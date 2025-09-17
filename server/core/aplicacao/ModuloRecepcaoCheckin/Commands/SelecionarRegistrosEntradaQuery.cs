using FluentResults;
using MediatR;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;

public record SelecionarRegistrosEntradaQuery(int? Quantidade)
    : IRequest<Result<SelecionarRegistrosEntradaResult>>;

public record SelecionarRegistrosEntradaResult(ImmutableList<SelecionarRegistrosEntradaDto> RegistrosEntrada);

public record SelecionarRegistrosEntradaDto(
    Guid Id,
    DateTime DataEntradaEmUtc,
    List<string>? Observacoes,
    Guid HospedeId,
    string NomeCompleto,
    Guid VeiculoId,
    string Placa
);
