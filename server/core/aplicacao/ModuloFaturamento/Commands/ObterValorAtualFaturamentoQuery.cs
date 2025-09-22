using FluentResults;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Commands;

public record ObterValorAtualFaturamentoQuery(
    Guid? EstacionamentoId,
    string? EstacionamentoNome,
    int? NumeroSequencialDoTicket,
    string? Placa
) : IRequest<Result<ObterValorAtualFaturamentoResult>>;

public record ObterValorAtualFaturamentoResult(
    int NumeroDeDiarias,
    decimal ValorDaDiaria,
    decimal ValorTotalAtual
);