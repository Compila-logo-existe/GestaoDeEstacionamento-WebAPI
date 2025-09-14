namespace GestaoDeEstacionamento.WebAPI.Models.ModuloRecepcaoCheckin;

public record ObterDetalhesVeiculoPorPlacaRequest(string Placa);

public record ObterDetalhesVeiculoPorPlacaResponse(
    Guid Id,
    string Placa,
    string Modelo,
    string Cor,
    string? Observacoes,
    string Hospede
);
