using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.WebAPI.Models.ModuloEstacionamento;

public record ConfigurarEstacionamentoRequest(
    string Nome,
    int QuantidadeVagas,
    int ZonasTotais,
    int VagasPorZona
);

public record ConfigurarEstacionamentoResponse(
    Guid Id,
    string Nome,
    int QuantidadeDeVagasCriadas,
    ImmutableList<ZonaEstacionamentoDto> Zonas
);