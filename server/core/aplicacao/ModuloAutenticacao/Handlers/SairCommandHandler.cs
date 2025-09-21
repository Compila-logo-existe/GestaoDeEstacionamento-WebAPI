using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;

public class SairCommandHandler(
    UserManager<Usuario> userManager,
    SignInManager<Usuario> signInManager,
    ITenantProvider tenantProvider,
    IRefreshTokenProvider refreshTokenProvider,
    IUnitOfWork unitOfWork,
    ILogger<SairCommand> logger
) : IRequestHandler<SairCommand, Result>
{
    public async Task<Result> Handle(
        SairCommand command, CancellationToken cancellationToken)
    {
        Guid? usuarioAutenticadoId = tenantProvider.UsuarioId;
        if (!usuarioAutenticadoId.HasValue || usuarioAutenticadoId.Value == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário autenticado não identificado."));

        try
        {
            await signInManager.SignOutAsync();

            await refreshTokenProvider.RevogarTokensUsuarioAsync(usuarioAutenticadoId.Value, cancellationToken);

            Usuario? usuario = await userManager.FindByIdAsync(usuarioAutenticadoId.Value.ToString());
            if (usuario is null)
                return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário não encontrado."));

            usuario.AccessTokenVersionId = Guid.NewGuid();

            IdentityResult resultado = await userManager.UpdateAsync(usuario);
            if (!resultado.Succeeded)
                return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Falha ao invalidar tokens de acesso."));

            await unitOfWork.CommitAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Ocorreu um erro durante a autenticação. {@Command}.",
                command
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }
}
