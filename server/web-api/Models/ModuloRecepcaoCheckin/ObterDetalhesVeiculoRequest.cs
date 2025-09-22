namespace GestaoDeEstacionamento.WebAPI.Models.ModuloRecepcaoCheckin;

public record ObterDetalhesVeiculoRequest(
    Guid? VeiculoId,
    string? Placa
);

public record ObterDetalhesVeiculoResponse(
    Guid Id,
    string Placa,
    string Modelo,
    string Cor,
    string? Observacoes,
    string Hospede
);
