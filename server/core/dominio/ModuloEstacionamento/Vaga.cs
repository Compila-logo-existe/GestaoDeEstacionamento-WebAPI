using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;

public class Vaga : EntidadeBase<Vaga>
{
    public int Numero { get; set; }
    public ZonaEstacionamento Zona { get; set; }
    public Guid? VeiculoId { get; set; }
    public Veiculo? Veiculo { get; set; }
    public StatusVaga Status => Veiculo is null ? StatusVaga.Livre : StatusVaga.Ocupada;
    public Guid EstacionamentoId { get; set; }
    public Estacionamento Estacionamento { get; set; } = null!;

    public void AderirUsuario(Guid usuarioId) => UsuarioId = usuarioId;

    public void Ocupar(Veiculo veiculo)
    {
        Veiculo = veiculo;
        VeiculoId = veiculo.Id;
    }

    public void Liberar()
    {
        Veiculo = null;
        VeiculoId = null;
    }

    public override void AtualizarRegistro(Vaga registroEditado)
    {
        Numero = registroEditado.Numero;
        Zona = registroEditado.Zona;
    }
}
