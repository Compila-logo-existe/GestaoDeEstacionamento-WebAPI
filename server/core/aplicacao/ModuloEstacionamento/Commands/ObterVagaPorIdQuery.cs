using FluentResults;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

public record ObterVagaPorIdQuery(
    Guid? EstacionamentoId,
    string? EstacionamentoNome,
    Guid? VagaId,
    int? VagaNumero,
    string? VagaZona
) : IRequest<Result<ObterVagaPorIdResult>>;

public record ObterVagaPorIdResult(ObterStatusVagasDto Vaga);
