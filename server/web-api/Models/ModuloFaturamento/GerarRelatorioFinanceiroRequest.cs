using GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Commands;

namespace GestaoDeEstacionamento.WebAPI.Models.ModuloFaturamento;

public record GerarRelatorioFinanceiroRequest(
    DateTime DataInicial,
    DateTime DataFinal
);

public record GerarRelatorioFinanceiroResponse(
    DateTime DataInicial,
    DateTime DataFinal,
    int QuantidadeFaturamentos,
    decimal ValorTotalPeriodo,
    IEnumerable<FaturamentoDto> Itens
);
