using FluentResults;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

public record ConfigurarEstacionamentoCommand(
    string Nome,
    int QuantidadeVagas
) : IRequest<Result<ConfigurarEstacionamentoResult>>;

public record ConfigurarEstacionamentoResult(Guid Id);
