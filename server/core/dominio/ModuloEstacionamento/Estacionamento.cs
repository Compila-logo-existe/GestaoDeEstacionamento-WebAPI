using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;

public class Estacionamento : EntidadeBase<Estacionamento>
{
    public string Nome { get; set; }
    public List<Vaga> Vagas { get; set; } = new();

    public Estacionamento() { }
    public Estacionamento(string nome, int quantidadeVagas) : this()
    {
        Nome = nome;
        Vagas = new(quantidadeVagas);
    }

    public void AderirUsuario(Guid usuarioId) => UsuarioId = usuarioId;

    public override void AtualizarRegistro(Estacionamento registroEditado)
    {
        Vagas = registroEditado.Vagas;
    }
}
