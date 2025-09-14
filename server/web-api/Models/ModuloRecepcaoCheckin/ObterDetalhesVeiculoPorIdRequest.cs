namespace GestaoDeEstacionamento.WebAPI.Models.ModuloRecepcaoCheckin;

public record ObterDetalhesVeiculoPorIdRequest(Guid Id);

public record ObterDetalhesVeiculoPorIdResponse(
    Guid Id,
    string Placa,
    string Modelo,
    string Cor,
    string? Observacoes,
    string Hospede
);
