using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public class Tenant : EntidadeBase<Tenant>
{
    public Guid UsuarioCriadorId { get; set; }
    public string Nome { get; set; }
    public string? CNPJ { get; set; }
    public string SlugSubdominio { get; private set; } = null!;
    public string? DominioPersonalizado { get; private set; }
    public DateTime CriadoEmUtc { get; set; }
    public bool Ativo { get; private set; } = true;

    public Tenant() { }
    public Tenant(Guid usuarioCriadorId, string nome, string? cNPJ,
        string slugSubdominio, string? dominioPersonalizado, DateTime criadoEmUtc) : this()
    {
        UsuarioCriadorId = usuarioCriadorId;
        Nome = nome;
        CNPJ = cNPJ;
        SlugSubdominio = slugSubdominio;
        DominioPersonalizado = dominioPersonalizado;
        CriadoEmUtc = criadoEmUtc;
    }

    public void Desativar() => Ativo = false;

    public override void AtualizarRegistro(Tenant registroEditado)
    {
        Nome = registroEditado.Nome;
        CNPJ = registroEditado.CNPJ;
        SlugSubdominio = registroEditado.SlugSubdominio;
        DominioPersonalizado = registroEditado.DominioPersonalizado;
    }
}
