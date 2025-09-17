using FluentResults;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

public record OcuparVagaCommand(
    Guid? EstacionamentoId,
    string? EstacionamentoNome,
    Guid? VagaId,
    int? VagaNumero,
    string? VagaZona,
    Guid? VeiculoId,
    string? Placa
) : IRequest<Result<OcuparVagaResult>>;

public record OcuparVagaResult(OcuparVagaDto Vaga);

public record OcuparVagaDto(
    bool Estacionou,
    string EstacionamentoNome,
    string IdentificacaoVaga
);
