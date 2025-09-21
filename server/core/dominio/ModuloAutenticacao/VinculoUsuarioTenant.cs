using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public class VinculoUsuarioTenant : EntidadeBase<VinculoUsuarioTenant>
{
    public Guid UsuarioVinculadoId { get; set; }
    public string NomeCargo { get; set; } = default!;
    public string Slug { get; set; } = default!;

    public VinculoUsuarioTenant() { }
    public VinculoUsuarioTenant(Guid usuarioVinculadoId, Guid tenantId, string nomeCargo, string slug) : this()
    {
        UsuarioVinculadoId = usuarioVinculadoId;
        TenantId = tenantId;
        NomeCargo = nomeCargo;
        Slug = slug;
    }

    public override void AtualizarRegistro(VinculoUsuarioTenant registroEditado)
    {
        NomeCargo = registroEditado.NomeCargo;
    }
}
