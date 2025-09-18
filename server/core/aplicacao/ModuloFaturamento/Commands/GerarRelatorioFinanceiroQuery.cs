using FluentResults;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Commands;

public record GerarRelatorioFinanceiroQuery(
    DateTime DataInicial,
    DateTime DataFinal
) : IRequest<Result<GerarRelatorioFinanceiroResult>>;

public record GerarRelatorioFinanceiroResult(
    DateTime DataInicial,
    DateTime DataFinal,
    int QuantidadeFaturamentos,
    decimal ValorTotalPeriodo,
    IEnumerable<FaturamentoDto> Itens
);

public record FaturamentoDto(
    Guid Id,
    Guid RegistroEntradaId,
    Guid? RegistroSaidaId,
    DateTime DataEntradaEmUtc,
    DateTime? DataSaidaEmUtc,
    decimal ValorDaDiaria,
    int NumeroDeDiarias,
    decimal ValorTotal
);
