using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public interface IRepositorioUsuarioTenant : IRepositorio<VinculoUsuarioTenant>
{
    public Task<bool> UsuarioPertenceAoTenantAsync(Guid usuarioId, Guid tenantId, CancellationToken ct = default);
    public Task<bool> UsuarioPertenceAoTenantAsync(Guid usuarioId, string slug, CancellationToken ct = default);
    public Task CriarVinculoAsync(Guid usuarioId, Guid? tenantId, string? slug, string nomeCargo, CancellationToken ct = default);
}
