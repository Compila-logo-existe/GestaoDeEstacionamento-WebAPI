using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

namespace GestaoDeEstacionamento.WebAPI.Models.ModuloEstacionamento;

public record ObterVagaPorIdRequest(
    Guid? EstacionamentoId,
    string? EstacionamentoNome,
    Guid? VagaId,
    int? VagaNumero,
    string? VagaZona
);

public record ObterVagaPorIdResponse(ObterStatusVagasDto Vaga);
