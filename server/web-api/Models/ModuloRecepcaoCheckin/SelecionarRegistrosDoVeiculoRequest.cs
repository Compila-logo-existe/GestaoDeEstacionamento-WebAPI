using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.WebAPI.Models.ModuloRecepcaoCheckin;

public record SelecionarRegistrosDoVeiculoRequest(
    int? Quantidade,
    Guid? VeiculoId,
    string? Placa
);

public record SelecionarRegistrosDoVeiculoResponse(
    int Quantidade,
    ImmutableList<SelecionarRegistrosEntradaDto> RegistrosEntrada
);
