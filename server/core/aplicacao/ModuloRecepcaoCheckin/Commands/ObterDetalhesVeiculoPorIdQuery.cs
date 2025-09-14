using FluentResults;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;

public record ObterDetalhesVeiculoPorIdQuery(Guid Id) : IRequest<Result<ObterDetalhesVeiculoPorIdResult>>;

public record ObterDetalhesVeiculoPorIdResult(
    Guid Id,
    string Placa,
    string Modelo,
    string Cor,
    string? Observacoes,
    string Hospede
);
