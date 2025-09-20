using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;

public class CriarConviteCommandHandler(
    IRepositorioConvite repositorioConvite,
    ITenantProvider tenantProvider,
    IUnitOfWork unitOfWork,
    ILogger<CriarConviteCommand> logger
) : IRequestHandler<CriarConviteCommand, Result<(string, DateTime)>>
{
    public async Task<Result<(string, DateTime)>> Handle(
        CriarConviteCommand command, CancellationToken ct)
    {
        Guid? tenantId = tenantProvider.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Tenant não informado. Envie o header 'X-Tenant-Id'."));

        Guid? usuarioId = tenantProvider.UsuarioId;
        if (!usuarioId.HasValue || usuarioId.Value == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário autenticado não identificado."));

        try
        {
            string tokenConvite = Convert.ToHexString(Guid.NewGuid().ToByteArray());
            DateTime expira = DateTime.UtcNow.AddDays(7);

            ConviteRegistro convite = new()
            {
                UsuarioId = usuarioId.Value,
                TenantId = command.TenantId,
                EmailConvidado = command.EmailConvidado,
                NomeCargo = command.NomeCargo,
                TokenConvite = tokenConvite,
                DataExpiracaoUtc = expira
            };

            await repositorioConvite.CriarAsync(convite, ct);

            await unitOfWork.CommitAsync();

            return Result.Ok((tokenConvite, expira));
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();

            logger.LogError(
                ex,
                "Ocorreu um erro durante a criação do convite {@Command}.",
                command
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }
}
