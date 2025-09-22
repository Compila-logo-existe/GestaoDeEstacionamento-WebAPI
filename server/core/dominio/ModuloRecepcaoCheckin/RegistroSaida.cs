using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloFaturamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using System.Diagnostics.CodeAnalysis;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

public class RegistroSaida : EntidadeBase<RegistroSaida>
{
    public DateTime DataSaidaEmUtc { get; set; }
    public Guid HospedeId { get; set; }
    public Hospede Hospede { get; set; }
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public Guid VeiculoId { get; set; }
    public Veiculo Veiculo { get; set; }
    public Faturamento Faturamento { get; set; }
    public List<string> Observacoes { get; set; } = new();

    [ExcludeFromCodeCoverage]
    public RegistroSaida() { }
    public RegistroSaida(Hospede hospede, Veiculo veiculo,
        List<string> observacoes) : this()
    {
        Hospede = hospede;
        Veiculo = veiculo;
        if (observacoes is not null)
        {
            Observacoes.AddRange(observacoes);
        }
    }

    public void AderirHospede(Hospede hospede) => Hospede = hospede;

    public void AderirVeiculo(Veiculo veiculo) => Veiculo = veiculo;

    public void AderirTicket(Ticket ticket)
    {
        TicketId = ticket.Id;
        Ticket = ticket;
        ticket.AderirRegistroSaida(this);
    }

    public override void AtualizarRegistro(RegistroSaida registroEditado)
    {
        if (registroEditado is null)
            return;

        DataSaidaEmUtc = registroEditado.DataSaidaEmUtc;
        Observacoes = registroEditado.Observacoes;
        HospedeId = registroEditado.HospedeId;
        VeiculoId = registroEditado.VeiculoId;
        TicketId = registroEditado.TicketId;
    }
}
