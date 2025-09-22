using FluentResults;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;

public record RegistrarEntradaCommand(
    Guid? HospedeId,
    string? NomeCompleto,
    string? CPF,
    string? Telefone,
    string Placa,
    string Modelo,
    string Cor,
    decimal ValorDiaria,
    List<string> Observacoes
) : IRequest<Result<RegistrarEntradaResult>>;

public record RegistrarEntradaResult(
    Guid Id,
    int NumeroSequencialDoTicket,
    string DataEntradaEmUtc
);