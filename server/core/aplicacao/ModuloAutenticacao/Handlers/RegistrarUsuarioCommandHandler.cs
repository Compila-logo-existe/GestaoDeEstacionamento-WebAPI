using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;

public class RegistrarUsuarioCommandHandler(
    UserManager<Usuario> userManager,
    IRepositorioUsuarioTenant repositorioUsuarioTenant,
    IRepositorioTenant repositorioTenant,
    ITenantProvider tenantProvider,
    ITokenProvider tokenProvider,
    IUnitOfWork unitOfWork,
    ILogger<RegistrarUsuarioCommandHandler> logger
) : IRequestHandler<RegistrarUsuarioCommand, Result<AccessToken>>
{
    public async Task<Result<AccessToken>> Handle(
        RegistrarUsuarioCommand command, CancellationToken cancellationToken)
    {
        Guid? tenantId = tenantProvider.TenantId;
        if ((!tenantId.HasValue || tenantId.Value == Guid.Empty) && string.IsNullOrWhiteSpace(tenantProvider.Slug))
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Tenant não informado. Envie o header 'X-Tenant-Id'."));

        if (!command.Senha.Equals(command.ConfirmarSenha))
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("A confirmação de senha falhou."));

        Usuario usuario = new()
        {
            FullName = command.NomeCompleto,
            UserName = command.Email,
            Email = command.Email
        };

        try
        {
            if (!string.IsNullOrWhiteSpace(command.Slug))
            {
                command = command with
                {
                    TenantId = await repositorioTenant.ObterTenantIdPorSubdominioAsync(command.Slug, cancellationToken)
                };
            }

            Tenant? tenant = await repositorioTenant.ObterPorIdAsync(command.TenantId!.Value, cancellationToken);

            if (tenant is null)
                return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Empresa (tenant) não encontrada."));

            IdentityResult usuarioResult = await userManager.CreateAsync(usuario, command.Senha);

            if (!usuarioResult.Succeeded)
            {
                IEnumerable<string> erros = usuarioResult.Errors.Select(err =>
                {
                    return err.Code switch
                    {
                        "DuplicateUserName" => "Já existe um usuário com esse nome.",
                        "DuplicateEmail" => "Já existe um usuário com esse e-mail.",
                        "PasswordTooShort" => "A senha é muito curta.",
                        "PasswordRequiresNonAlphanumeric" => "A senha deve conter pelo menos um caractere especial.",
                        "PasswordRequiresDigit" => "A senha deve conter pelo menos um número.",
                        "PasswordRequiresUpper" => "A senha deve conter pelo menos uma letra maiúscula.",
                        "PasswordRequiresLower" => "A senha deve conter pelo menos uma letra minúscula.",
                        _ => err.Description
                    };
                });

                return Result.Fail(ResultadosErro.RequisicaoInvalidaErro(erros));
            }
            await userManager.AddToRoleAsync(usuario, "User");

            VinculoUsuarioTenant vinculo = new(
                usuario.Id,
                tenant.Id,
                "User",
                tenant.SlugSubdominio!
            );

            AccessToken? tokenAcesso = await tokenProvider.GerarAccessToken(
                usuario,
                tenant.Id
            );

            if (tokenAcesso is null)
            {
                await unitOfWork.RollbackAsync();

                return Result.Fail(ResultadosErro.ExcecaoInternaErro(new Exception("Falha ao gerar token de acesso.")));
            }

            await repositorioUsuarioTenant.CadastrarRegistroAsync(vinculo);

            await unitOfWork.CommitAsync();

            return Result.Ok(tokenAcesso);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();

            logger.LogError(
                ex,
                "Ocorreu um erro durante o registro. {@Command}.",
                command
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }
}
