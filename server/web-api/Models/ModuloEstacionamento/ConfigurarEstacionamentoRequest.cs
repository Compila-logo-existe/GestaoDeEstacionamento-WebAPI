namespace GestaoDeEstacionamento.WebAPI.Models.ModuloEstacionamento;

public record ConfigurarEstacionamentoRequest(
    string Nome,
    int QuantidadeVagas);

public record ConfigurarEstacionamentoResponse(Guid Id);
