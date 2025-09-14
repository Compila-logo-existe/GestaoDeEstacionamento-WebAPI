using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.WebAPI.Models.ModuloRecepcaoCheckin;

public record SelecionarRegistrosEntradaRequest(int? Quantidade);

public record SelecionarRegistrosEntradaResponse(
    int Quantidade,
    ImmutableList<SelecionarRegistrosEntradaDto> RegistrosEntrada
);
