using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

namespace GestaoDeEstacionamento.WebAPI.Models.ModuloEstacionamento;

public record ObterVagaPorIdRequest(Guid VagaId);

public record ObterVagaPorIdResponse(ObterStatusVagasDto Vaga);
