using FluentResults;
using MediatR;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;

public record SelecionarRegistrosDoVeiculoQuery(
    int? Quantidade,
    Guid? VeiculoId,
    string? Placa
) : IRequest<Result<SelecionarRegistrosDoVeiculoResult>>;

public record SelecionarRegistrosDoVeiculoResult(ImmutableList<SelecionarRegistrosEntradaDto> RegistrosEntrada);
