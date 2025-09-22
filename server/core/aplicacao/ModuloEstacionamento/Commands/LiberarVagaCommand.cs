using FluentResults;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

public record LiberarVagaCommand(
    Guid? EstacionamentoId,
    string? EstacionamentoNome,
    Guid? VagaId,
    int? VagaNumero,
    string? VagaZona,
    string? Placa
) : IRequest<Result<LiberarVagaResult>>;

public record LiberarVagaResult(LiberarVagaDto Vaga);

public record LiberarVagaDto(
    bool Desocupou,
    string EstacionamentoNome,
    string IdentificacaoVaga
);