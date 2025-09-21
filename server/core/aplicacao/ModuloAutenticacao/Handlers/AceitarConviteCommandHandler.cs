using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using MediatR;
using Microsoft.AspNetCore.Identity;
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
            return Result.Fail("Convite inválido ou expirado.");

        if (command.Senha != command.ConfirmarSenha)
            return Result.Fail("A confirmação de senha não confere.");

        try
        {
            Usuario usuario = new()
            {
                UserName = convite.EmailConvidado,
                Email = convite.EmailConvidado,
                FullName = command.NomeCompleto
            };

            IdentityResult criado = await userManager.CreateAsync(usuario, command.Senha);
            if (!criado.Succeeded)
                return Result.Fail(criado.Errors.Select(e => e.Description).ToArray());

            await userManager.AddToRoleAsync(usuario, convite.NomeCargo);

            Tenant? tenant = await repositorioTenant.ObterPorIdAsync(convite.TenantId, cancellationToken);

            if (tenant is null)
                return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Empresa (tenant) não encontrada."));

            VinculoUsuarioTenant vinculo = new(
                usuario.Id,
                convite.TenantId,
                "User",
                tenant.SlugSubdominio
            );

            await repositorioUsuarioTenant.CadastrarRegistroAsync(vinculo);

            await repositorioConvite.MarcarComoUtilizadoAsync(convite.Id, cancellationToken);

            await unitOfWork.CommitAsync();

            AccessToken token = await tokenProvider.GerarAccessToken(usuario, convite.TenantId);

            return Result.Ok(token);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();

            logger.LogError(
                ex,
                "Ocorreu um erro durante ao aceitar o convite {@Convite}.",
                convite
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }
}
