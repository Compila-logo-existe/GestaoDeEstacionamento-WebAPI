using FluentResults;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;

public record ObterDetalhesVeiculoQuery(
    Guid? VeiculoId,
    string? Placa
) : IRequest<Result<ObterDetalhesVeiculoResult>>;

public record ObterDetalhesVeiculoResult(
    Guid Id,
    string Placa,
    string Modelo,
    string Cor,
    string? Observacoes,
    string Hospede
);
