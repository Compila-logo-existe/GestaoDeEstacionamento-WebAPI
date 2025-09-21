using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using System.Diagnostics.CodeAnalysis;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloFaturamento;

public class Faturamento : EntidadeBase<Faturamento>
{
    public Guid? RegistroSaidaId { get; set; }
    public RegistroSaida? RegistroSaida { get; set; } = null!;
    public Guid RegistroEntradaId { get; set; }
    public RegistroEntrada RegistroEntrada { get; set; } = null!;
    public decimal ValorDaDiaria { get; set; }
    public int NumeroDeDiarias { get; private set; }
    public decimal ValorTotal { get; private set; }
    public DateTime DataEntradaEmUtc { get; set; }

    [ExcludeFromCodeCoverage]
    public Faturamento() { }

    public void DefinirValorDiaria(decimal valor)
    {
        ValorDaDiaria = valor;
        RecalcularTotais();
    }

    public void AderirRegistroEntrada(RegistroEntrada registroEntrada)
    {
        if (registroEntrada is null)
            return;

        RegistroEntrada = registroEntrada;
        RegistroEntradaId = registroEntrada.Id;
        DataEntradaEmUtc = registroEntrada.Ticket.ObterDataEntrada();
    }

    public void RegistrarSaida(RegistroSaida registroSaida)
    {
        RegistroSaida = registroSaida;
        RegistroSaidaId = registroSaida.Id;
        RegistroSaida.DataSaidaEmUtc = DateTime.UtcNow;
        RecalcularTotais();
    }

    public void RecalcularTotais()
    {
        DateTime dataFim = RegistroSaida?.DataSaidaEmUtc ?? DateTime.UtcNow;

        TimeSpan tempoDecorrido = dataFim - DataEntradaEmUtc;
        if (tempoDecorrido <= TimeSpan.Zero)
        {
            NumeroDeDiarias = 1;
        }
        else
        {
            TimeSpan umDia = TimeSpan.FromDays(1);

            long ticksDecorridos = tempoDecorrido.Ticks;
            long ticksPorDia = umDia.Ticks;

            long diasInteiros = ticksDecorridos / ticksPorDia;
            bool possuiRestoDeDia = (ticksDecorridos % ticksPorDia) != 0;

            int diarias = (int)diasInteiros + (possuiRestoDeDia ? 1 : 0);

            NumeroDeDiarias = Math.Max(1, diarias);
        }

        ValorTotal = NumeroDeDiarias * ValorDaDiaria;
    }

    public override void AtualizarRegistro(Faturamento registroEditado)
    {
        ValorDaDiaria = registroEditado.ValorDaDiaria;
        NumeroDeDiarias = registroEditado.NumeroDeDiarias;
        ValorTotal = registroEditado.ValorTotal;
    }
}
