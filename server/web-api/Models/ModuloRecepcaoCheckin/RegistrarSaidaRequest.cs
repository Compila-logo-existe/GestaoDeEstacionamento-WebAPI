namespace GestaoDeEstacionamento.WebAPI.Models.ModuloRecepcaoCheckin;

public record RegistrarSaidaRequest(
    Guid? HospedeId,
    string? CPF,
    int NumeroSequencialDoTicket,
    Guid? VeiculoId,
    string? Placa
);

public record RegistrarSaidaResponse(DateTime DataSaidaEmUtc);
