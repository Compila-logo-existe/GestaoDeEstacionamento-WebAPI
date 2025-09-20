using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public interface IUsuarioTenantRepositorio : IRepositorio<VinculoUsuarioTenant>
{
    public Task<bool> UsuarioPertenceAoTenantAsync(Guid usuarioId, Guid tenantId, CancellationToken cancellationToken);
    public Task<bool> UsuarioPertenceAoTenantAsync(Guid usuarioId, string slug, CancellationToken cancellationToken);
    public Task CriarVinculoAsync(Guid usuarioId, Guid? tenantId, string? slug, string nomeCargo, CancellationToken cancellationToken);
}
