using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.WebAPI.Models.ModuloEstacionamento;

public record ObterStatusVagasRequest(
    int? Quantidade,
    Guid? EstacionamentoId,
    string? EstacionamentoNome,
    string? Zona,
    string? Placa
);

public record ObterStatusVagasResponse(
    int Quantidade,
    ImmutableList<ObterStatusVagasDto> Vagas
);
