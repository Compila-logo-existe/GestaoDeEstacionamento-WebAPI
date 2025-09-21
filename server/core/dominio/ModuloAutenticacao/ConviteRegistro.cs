using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public class ConviteRegistro : EntidadeBase<ConviteRegistro>
{
    public Guid UsuarioEmissorId { get; set; }
    public string EmailConvidado { get; set; } = null!;
    public string NomeCargo { get; set; } = null!;
    public string TokenConvite { get; set; } = null!;
    public DateTime DataExpiracaoUtc { get; set; }
    public DateTime? UtilizadoEmUtc { get; private set; }

    public ConviteRegistro() { }
    public ConviteRegistro(Guid usuarioEmissorId, Guid tenantId, string emailConvidado,
        string nomeCargo, string tokenConvite, DateTime dataExpiracaoUtc) : this()
    {
        UsuarioEmissorId = usuarioEmissorId;
        TenantId = tenantId;
        EmailConvidado = emailConvidado;
        NomeCargo = nomeCargo;
        TokenConvite = tokenConvite;
        DataExpiracaoUtc = dataExpiracaoUtc;
    }

    public bool EstaValidoAgora() => UtilizadoEmUtc is null && DateTime.UtcNow <= DataExpiracaoUtc;
    public void MarcarComoUtilizado() => UtilizadoEmUtc = DateTime.UtcNow;

    public override void AtualizarRegistro(ConviteRegistro registroEditado)
    {
        NomeCargo = registroEditado.NomeCargo;
        DataExpiracaoUtc = registroEditado.DataExpiracaoUtc;
    }
}
