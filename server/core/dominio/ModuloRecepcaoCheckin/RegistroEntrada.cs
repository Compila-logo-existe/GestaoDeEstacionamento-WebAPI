using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using System.Diagnostics.CodeAnalysis;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

public class RegistroEntrada : EntidadeBase<RegistroEntrada>
{
    public DateTime DataEntradaEmUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DataSaidaEmUtc { get; private set; }
    public Guid HospedeId { get; set; }
    public Hospede Hospede { get; set; }
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public Guid VeiculoId { get; set; }
    public Veiculo Veiculo { get; set; }
    public string? Observacoes { get; set; }

    [ExcludeFromCodeCoverage]
    public RegistroEntrada() { }
    public RegistroEntrada(Hospede hospede, Veiculo veiculo,
        string? observacoes) : this()
    {
        Hospede = hospede;
        Veiculo = veiculo;
        Observacoes = observacoes;
    }

    public void AderirUsuario(Guid usuarioId) => UsuarioId = usuarioId;

    public void AderirHospede(Hospede hospede) => Hospede = hospede;

    public void AderirVeiculo(Veiculo veiculo) => Veiculo = veiculo;

    public void GerarNovoTicket() => Ticket = new Ticket(this);

    public void AderirUsuarioAoTicket(Guid usuarioId) => Ticket.UsuarioId = usuarioId;

    public void RegistrarSaida() => DataSaidaEmUtc = DateTime.UtcNow;

    public override void AtualizarRegistro(RegistroEntrada registroEditado)
    {
        Observacoes = registroEditado.Observacoes;
    }
}
