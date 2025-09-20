using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;

public class RepositorioTenant(AppDbContext contexto)
    : RepositorioBaseORM<Tenant>(contexto), IRepositorioTenant
{
    public override async Task<Tenant?> SelecionarRegistroPorIdAsync(Guid idRegistro)
    {
        return await registros.Where(t => t.Id.Equals(idRegistro))
            .FirstOrDefaultAsync();
    }

    public async Task CriarAsync(Tenant tenant, CancellationToken ct)
    {
        await registros.AddAsync(tenant, ct);
    }

    public async Task<Tenant?> ObterPorIdAsync(Guid tenantId, CancellationToken ct)
    {
        return await registros.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id.Equals(tenantId), ct);
    }

    public async Task<Guid?> ObterTenantIdPorSubdominioAsync(string slug, CancellationToken ct)
    {
        return await registros.AsNoTracking()
             .Where(t => t.SlugSubdominio == slug)
             .Select(t => (Guid?)t.Id)
             .FirstOrDefaultAsync(ct);
    }

    public async Task<Guid?> ObterTenantIdPorDominioAsync(string host, CancellationToken ct)
    {
        return await registros.AsNoTracking()
              .Where(t => t.DominioPersonalizado == host)
              .Select(t => (Guid?)t.Id)
              .FirstOrDefaultAsync(ct);
    }
}
