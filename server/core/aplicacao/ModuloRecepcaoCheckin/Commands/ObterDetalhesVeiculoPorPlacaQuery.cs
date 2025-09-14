using FluentResults;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;

public record ObterDetalhesVeiculoPorPlacaQuery(string Placa) : IRequest<Result<ObterDetalhesVeiculoPorPlacaResult>>;

public record ObterDetalhesVeiculoPorPlacaResult(
    Guid Id,
    string Placa,
    string Modelo,
    string Cor,
    string? Observacoes,
    string Hospede
);
