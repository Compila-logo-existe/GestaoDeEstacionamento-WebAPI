using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;

public class AceitarConviteCommandHandler(
    IRepositorioConvite repositorioConvite,
    IRepositorioTenant repositorioTenant,
    IRepositorioUsuarioTenant repositorioUsuarioTenant,
    UserManager<Usuario> userManager,
    ITokenProvider tokenProvider,
    IUnitOfWork unitOfWork,
    ILogger<AceitarConviteCommand> logger
) : IRequestHandler<AceitarConviteCommand, Result<AccessToken>>
{
    public async Task<Result<AccessToken>> Handle(
        AceitarConviteCommand command, CancellationToken cancellationToken)
    {
        ConviteRegistro? convite = await repositorioConvite.ObterAtivoPorTokenAsync(command.TokenConvite, cancellationToken);
        if (convite?.EstaValidoAgora() != true)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Convite inválido ou expirado."));

        if (string.IsNullOrWhiteSpace(command.Senha) || command.Senha != command.ConfirmarSenha)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("A confirmação de senha não confere."));

        try
        {
            string email = convite.EmailConvidado.Trim();
            Usuario? usuario = await userManager.FindByEmailAsync(email);

            if (usuario is null)
            {
                usuario = new Usuario
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = string.IsNullOrWhiteSpace(command.NomeCompleto) ? email : command.NomeCompleto.Trim(),
                    AccessTokenVersionId = Guid.NewGuid()
                };

                IdentityResult criado = await userManager.CreateAsync(usuario, command.Senha);
                if (!criado.Succeeded)
                    return Result.Fail(criado.Errors.Select(e => $"{e.Code};{e.Description}").ToArray());
            }
            else
            {
                bool senhaOk = await userManager.CheckPasswordAsync(usuario, command.Senha);
                if (!senhaOk)
                    return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Senha incorreta para o usuário convidado."));

                if (string.IsNullOrWhiteSpace(usuario.FullName) && !string.IsNullOrWhiteSpace(command.NomeCompleto))
                {
                    usuario.FullName = command.NomeCompleto.Trim();
                    await userManager.UpdateAsync(usuario);
                }
            }

            IList<string> rolesDoUsuario = await userManager.GetRolesAsync(usuario);
            if (!rolesDoUsuario.Any(r => string.Equals(r, convite.NomeCargo, StringComparison.OrdinalIgnoreCase)))
                await userManager.AddToRoleAsync(usuario, convite.NomeCargo);

            Tenant? tenant = await repositorioTenant.ObterPorIdAsync(convite.TenantId, cancellationToken);
            if (tenant is null)
                return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Empresa (tenant) não encontrada."));

            bool jaVinculado = await repositorioUsuarioTenant.UsuarioPertenceAoTenantAsync(usuario.Id, convite.TenantId, cancellationToken);
            if (!jaVinculado)
            {
                VinculoUsuarioTenant vinculo = new()
                {
                    UsuarioVinculadoId = usuario.Id,
                    NomeCargo = convite.NomeCargo,
                    Slug = tenant.SlugSubdominio
                };
                vinculo.VincularTenant(convite.TenantId);

                await repositorioUsuarioTenant.CadastrarRegistroAsync(vinculo);
            }

            await repositorioConvite.MarcarComoUtilizadoAsync(convite.Id, cancellationToken);

            await unitOfWork.CommitAsync();

            AccessToken token = await tokenProvider.GerarAccessToken(usuario, convite.TenantId);

            return Result.Ok(token);
        }
        catch (DbUpdateException)
        {
            await unitOfWork.RollbackAsync();

            return Result.Fail(ResultadosErro.RegistroDuplicadoErro("Usuário já está vinculado a este tenant."));
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();

            logger.LogError(
                ex,
                "Erro ao aceitar convite {Token}",
                command.TokenConvite
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }
}
