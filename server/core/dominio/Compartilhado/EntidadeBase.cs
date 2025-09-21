namespace GestaoDeEstacionamento.Core.Dominio.Compartilhado;

public abstract class EntidadeBase<Tipo>
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }

    public void VincularTenant(Guid tenantId) => TenantId = tenantId;

    public abstract void AtualizarRegistro(Tipo registroEditado);
}
