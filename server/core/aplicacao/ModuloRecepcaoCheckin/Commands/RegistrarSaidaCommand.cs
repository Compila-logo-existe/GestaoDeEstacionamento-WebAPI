using FluentResults;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
public record RegistrarSaidaCommand(
    Guid? HospedeId,
    string? CPF,
    int NumeroSequencialDoTicket,
    Guid? VeiculoId,
    string? Placa
) : IRequest<Result<RegistrarSaidaResult>>;

public record RegistrarSaidaResult(string DataSaidaEmUtc);
