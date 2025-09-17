using FluentResults;
using MediatR;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

public record ConfigurarEstacionamentoCommand(
    string Nome,
    int QuantidadeVagas,
    int ZonasTotais,
    int VagasPorZona
) : IRequest<Result<ConfigurarEstacionamentoResult>>;

public record ConfigurarEstacionamentoResult(
    Guid Id,
    string Nome,
    int QuantidadeDeVagasCriadas,
    ImmutableList<ZonaEstacionamentoDto> Zonas
);

public record ZonaEstacionamentoDto(string Zona, int NumeroVaga);
