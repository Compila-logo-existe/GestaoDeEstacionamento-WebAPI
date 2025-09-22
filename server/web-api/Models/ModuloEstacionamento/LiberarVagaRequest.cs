using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

namespace GestaoDeEstacionamento.WebAPI.Models.ModuloEstacionamento;

public record LiberarVagaRequest(
    Guid? EstacionamentoId,
    string? EstacionamentoNome,
    Guid? VagaId,
    int? VagaNumero,
    string? VagaZona,
    string? Placa
);

public record LiberarVagaResponse(LiberarVagaDto Vaga);
