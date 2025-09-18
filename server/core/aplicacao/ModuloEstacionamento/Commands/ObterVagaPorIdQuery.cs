using FluentResults;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

public record ObterVagaPorIdQuery(Guid VagaId)
    : IRequest<Result<ObterVagaPorIdResult>>;

public record ObterVagaPorIdResult(ObterStatusVagasDto Vaga);
