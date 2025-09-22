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

    public void VincularTenant(Guid tenantId) => TenantId = tenantId;

    public override void AtualizarRegistro(RefreshToken registroEditado)
    {
        if (registroEditado is null)
            return;

        EnderecoIpDeCriacao = registroEditado.EnderecoIpDeCriacao;
        UserAgentDeCriacao = registroEditado.UserAgentDeCriacao;
    }
}
