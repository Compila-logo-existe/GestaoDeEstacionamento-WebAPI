namespace GestaoDeEstacionamento.WebAPI.Models.ModuloFaturamento;

public record ObterValorAtualFaturamentoRequest(
    Guid? EstacionamentoId,
    string? EstacionamentoNome,
    int? NumeroSequencialDoTicket,
    string? Placa
);

public record ObterValorAtualFaturamentoResponse(
    int NumeroDeDiarias,
    decimal ValorDaDiaria,
    decimal ValorTotalAtual
);
