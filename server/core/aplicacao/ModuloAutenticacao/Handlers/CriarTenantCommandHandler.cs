using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;

public class CriarTenantCommandHandler(
    IRepositorioTenant repositorioTenant,
    ITenantProvider tenantProvider,
    IUnitOfWork unitOfWork,
    ILogger<CriarTenantCommandHandler> logger
) : IRequestHandler<CriarTenantCommand, Result<TenantResult>>
{
    public async Task<Result<TenantResult>> Handle(
        CriarTenantCommand command, CancellationToken ct)
    {
        Guid? usuarioId = tenantProvider.UsuarioId;
        if (usuarioId is null || usuarioId == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário não identificado no tenant."));

        try
        {
            Tenant tenant = new(
                tenantProvider.UsuarioId!.Value, command.Nome, command.CNPJ,
                command.SlugSubdominio.ToLowerInvariant(), command.DominioPersonalizado?.ToLowerInvariant(),
                DateTime.UtcNow
            );

            await repositorioTenant.CriarAsync(tenant, ct);
            await unitOfWork.CommitAsync();

            TenantResult result = new(tenant.Id, tenant.SlugSubdominio);

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();

            logger.LogError(
                ex,
                "Ocorreu um erro durante a criação do tenant {@Command}.",
                command
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }
}
