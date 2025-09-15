using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;

public class Vaga : EntidadeBase<Vaga>
{
    public int Numero { get; set; }
    public ZonaEstacionamento Zona { get; set; }
    public Guid VeiculoId { get; set; }
    public Veiculo VeiculoEstacionado { get; set; } = null!;
    public StatusVaga Status => VeiculoEstacionado is null ? StatusVaga.Livre : StatusVaga.Ocupada;
    public Guid EstacionamentoId { get; set; }
    public Estacionamento Estacionamento { get; set; } = null!;

    public void AderirUsuario(Guid usuarioId) => UsuarioId = usuarioId;

    public void AderirVeiculo(Veiculo veiculo) => VeiculoEstacionado = veiculo;

    public void AderirEstacionamento(Estacionamento estacionamento) => Estacionamento = estacionamento;

    public override void AtualizarRegistro(Vaga registroEditado)
    {
        Numero = registroEditado.Numero;
        Zona = registroEditado.Zona;
    }
}
