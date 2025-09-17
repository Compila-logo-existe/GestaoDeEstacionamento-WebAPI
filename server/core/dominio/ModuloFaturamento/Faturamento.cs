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

    public void AderirUsuario(Guid usuarioId) => UsuarioId = usuarioId;

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
        if (RegistroSaida?.DataSaidaEmUtc is null)
            return;

        DateTime inicio = DataEntradaEmUtc.Date;
        DateTime fim = RegistroSaida.DataSaidaEmUtc.Value.Date;

        int dias = (fim - inicio).Days + 1;
        NumeroDeDiarias = Math.Max(1, dias);

        ValorTotal = NumeroDeDiarias * ValorDaDiaria;
    }

    public override void AtualizarRegistro(Faturamento registroEditado)
    {
        ValorDaDiaria = registroEditado.ValorDaDiaria;
        NumeroDeDiarias = registroEditado.NumeroDeDiarias;
        ValorTotal = registroEditado.ValorTotal;
    }
}
