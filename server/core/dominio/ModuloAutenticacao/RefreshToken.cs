using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public class RefreshToken : EntidadeBase<RefreshToken>
{
    public Guid UsuarioAutenticadoId { get; set; }
    public string HashDoToken { get; set; } = string.Empty;
    public DateTime CriadoEmUtc { get; set; }
    public DateTime ExpiraEmUtc { get; set; }
    public DateTime? RevogadoEmUtc { get; set; }
    public string? SubstituidoPorHashDoToken { get; set; }
    public string? EnderecoIpDeCriacao { get; set; }
    public string? UserAgentDeCriacao { get; set; }

    public bool EstaAtivo => RevogadoEmUtc is null && DateTime.UtcNow <= ExpiraEmUtc;

    public void VincularTenant(Guid tenantId) => UsuarioId = tenantId;

    public override void AtualizarRegistro(RefreshToken registroEditado)
    {
        EnderecoIpDeCriacao = registroEditado.EnderecoIpDeCriacao;
        UserAgentDeCriacao = registroEditado.UserAgentDeCriacao;
    }
}
