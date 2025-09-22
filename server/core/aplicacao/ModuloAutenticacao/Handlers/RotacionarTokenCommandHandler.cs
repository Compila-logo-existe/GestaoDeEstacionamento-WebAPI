using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;

public class RotacionarTokenCommandHandler(
    IUnitOfWork unitOfWork,
    ITokenProvider jwtProvider,
    IRefreshTokenProvider refreshTokenProvider,
    ILogger<RotacionarTokenCommandHandler> logger
) : IRequestHandler<RotacionarTokenCommand, Result<(AccessToken, string)>>
{
    public async Task<Result<(AccessToken, string)>> Handle(
        RotacionarTokenCommand command, CancellationToken cancellationToken)
    {
        Result<(Usuario Usuario, Guid TenantId, string NovoRefreshToken)> rotacao =
            await refreshTokenProvider.RotacionarRefreshTokenAsync(command.RefreshTokenString, cancellationToken);

        if (rotacao.IsFailed)
            return Result.Fail(rotacao.Errors);

        try
        {
            Usuario usuario = rotacao.Value.Usuario;
            Guid tenantId = rotacao.Value.TenantId;

            usuario.AccessTokenVersionId = Guid.NewGuid();

            AccessToken novoAccessToken = await jwtProvider.GerarAccessToken(usuario, tenantId);

            await unitOfWork.CommitAsync();

            return Result.Ok((novoAccessToken, rotacao.Value.NovoRefreshToken));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Ocorreu um erro durante a rotação de token. {@Command}.",
                command
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }
}