using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public interface IRepositorioTenant : IRepositorio<Tenant>
{
    public Task CriarAsync(Tenant tenant, CancellationToken ct = default);
    public Task<Tenant?> ObterPorIdAsync(Guid tenantId, CancellationToken ct = default);
    public Task<Guid?> ObterTenantIdPorSubdominioAsync(string slug, CancellationToken ct = default);
    public Task<Guid?> ObterTenantIdPorDominioAsync(string host, CancellationToken ct = default);

}
