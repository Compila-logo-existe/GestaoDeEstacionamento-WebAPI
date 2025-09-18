using FluentResults;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using MediatR;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

public record ObterStatusVagasQuery(
    int? Quantidade,
    Guid? EstacionamentoId,
    string? EstacionamentoNome,
    string? Zona,
    string? Placa
) : IRequest<Result<ObterStatusVagasResult>>;

public record ObterStatusVagasResult(ImmutableList<ObterStatusVagasDto> Vagas);

public record ObterStatusVagasDto(
    Guid Id,
    int Numero,
    ZonaEstacionamento Zona,
    StatusVaga Status,
    string? Placa
);
