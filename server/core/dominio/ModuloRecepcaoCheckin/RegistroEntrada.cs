using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloFaturamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using System.Diagnostics.CodeAnalysis;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

public class RegistroEntrada : EntidadeBase<RegistroEntrada>
{
    public DateTime DataEntradaEmUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DataSaidaEmUtc { get; set; }
    public Guid HospedeId { get; set; }
    public Hospede Hospede { get; set; }
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public Guid VeiculoId { get; set; }
    public Veiculo Veiculo { get; set; }
    public Faturamento Faturamento { get; set; }
    public List<string> Observacoes { get; set; } = new();

    [ExcludeFromCodeCoverage]
    public RegistroEntrada() { }
    public RegistroEntrada(Hospede hospede, Veiculo veiculo,
        string? observacao, List<string> observacoes) : this()
    {
        Hospede = hospede;
        Veiculo = veiculo;
        if (observacoes != null)
        {
            Observacoes.AddRange(observacoes);
        }
    }

    public void VincularTenantAoTicket(Guid tenantId) => Ticket.VincularTenant(tenantId);

    public void AderirHospede(Hospede hospede) => Hospede = hospede;

    public void AderirVeiculo(Veiculo veiculo) => Veiculo = veiculo;

    public void GerarNovoTicket() => Ticket = new Ticket(this);

    public void AderirDataSaida(DateTime dataSaidaEmUtc) => DataSaidaEmUtc = dataSaidaEmUtc;

    public void GerarNovoFaturamento(decimal valorDaDiaria)
    {
        Faturamento = new Faturamento();
        Faturamento.VincularTenant(TenantId);
        Faturamento.AderirRegistroEntrada(this);
        Faturamento.DefinirValorDiaria(valorDaDiaria);
    }

    public override void AtualizarRegistro(RegistroEntrada registroEditado)
    {
        Observacoes = registroEditado.Observacoes;
    }
}
