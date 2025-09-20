using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public class VinculoUsuarioTenant : EntidadeBase<VinculoUsuarioTenant>
{
    public Guid TenantId { get; set; }
    public string NomeCargo { get; set; } = null!;
    public string Slug { get; set; } = null!;

    public VinculoUsuarioTenant() { }
    public VinculoUsuarioTenant(Guid usuarioVinculadoId, Guid tenantId, string nomeCargo, string slug) : this()
    {
        UsuarioId = usuarioVinculadoId;
        TenantId = tenantId;
        NomeCargo = nomeCargo;
        Slug = slug;
    }

    public override void AtualizarRegistro(VinculoUsuarioTenant registroEditado)
    {
        NomeCargo = registroEditado.NomeCargo;
    }
}
