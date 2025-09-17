using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

namespace GestaoDeEstacionamento.WebAPI.Models.ModuloEstacionamento;

public record OcuparVagaRequest(
    Guid? EstacionamentoId,
    string? EstacionamentoNome,
    Guid? VagaId,
    int? VagaNumero,
    string? VagaZona,
    Guid? VeiculoId,
    string? Placa
);

public record OcuparVagaResponse(OcuparVagaDto Vaga);
