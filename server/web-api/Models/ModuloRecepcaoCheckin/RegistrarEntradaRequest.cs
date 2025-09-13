namespace GestaoDeEstacionamento.WebAPI.Models.ModuloRecepcaoCheckin;

public record RegistrarEntradaRequest(
    DateTime DataEntrada,
    string NomeCompleto,
    string CPF,
    string Telefone,
    string Placa,
    string Modelo,
    string Cor,
    string? Observacoes
);

public record RegistrarEntradaResponse(Guid Id, int NumeroSequencialDoTicket);
