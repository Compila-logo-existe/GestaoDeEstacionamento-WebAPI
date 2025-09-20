using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public interface ITenantRepositorio : IRepositorio<Tenant>
{
    public Task CriarAsync(Tenant tenant, CancellationToken ct);
    public Task<Tenant?> ObterPorIdAsync(Guid tenantId, CancellationToken ct);
    public Task<Guid?> ObterTenantIdPorSubdominioAsync(string slug, CancellationToken ct);
    public Task<Guid?> ObterTenantIdPorDominioAsync(string host, CancellationToken ct);

}
