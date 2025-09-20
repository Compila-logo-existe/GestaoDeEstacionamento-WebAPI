using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;

public class RepositorioUsuarioTenant(AppDbContext contexto)
    : RepositorioBaseORM<VinculoUsuarioTenant>(contexto), IRepositorioUsuarioTenant
{
    public override async Task<VinculoUsuarioTenant?> SelecionarRegistroPorIdAsync(Guid idRegistro)
    {
        return await registros
            .Where(v => v.Id.Equals(idRegistro))
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UsuarioPertenceAoTenantAsync(Guid usuarioId, Guid tenantId, CancellationToken cancellationToken)
    {
        return await registros
            .AsNoTracking()
            .AnyAsync(v => v.UsuarioId.Equals(usuarioId) && v.TenantId.Equals(tenantId), cancellationToken);
    }

    public async Task<bool> UsuarioPertenceAoTenantAsync(Guid usuarioId, string slug, CancellationToken cancellationToken)
    {
        return await registros
            .AsNoTracking()
            .AnyAsync(v => v.UsuarioId.Equals(usuarioId) && v.Slug.Equals(slug), cancellationToken);
    }

    public async Task CriarVinculoAsync(Guid usuarioId, Guid? tenantId, string? slug, string nomeCargo, CancellationToken cancellationToken)
    {
        VinculoUsuarioTenant vinculo = new(
            usuarioId,
            tenantId!.Value,
            nomeCargo,
            slug!
        );

        await registros.AddAsync(vinculo, cancellationToken);
    }
}
