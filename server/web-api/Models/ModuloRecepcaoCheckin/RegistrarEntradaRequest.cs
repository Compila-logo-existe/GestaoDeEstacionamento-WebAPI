namespace GestaoDeEstacionamento.WebAPI.Models.ModuloRecepcaoCheckin;

public record RegistrarEntradaRequest(
    Guid? HospedeId,
    string? NomeCompleto,
    string? CPF,
    string? Telefone,
    string Placa,
    string Modelo,
    string Cor,
    decimal ValorDiaria,
    List<string> Observacoes
);

public record RegistrarEntradaResponse(
    Guid Id,
    int NumeroSequencialDoTicket,
    DateTime DataEntradaEmUtc
);
