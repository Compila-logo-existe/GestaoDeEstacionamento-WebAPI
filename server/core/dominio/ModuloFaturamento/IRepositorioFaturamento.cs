using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloFaturamento;

public interface IRepositorioFaturamento : IRepositorio<Faturamento>
{
    public Task<List<Faturamento>> SelecionarPorPeriodoAsync(DateTime dataInicialEmUtc, DateTime dataFinalEmUtc, Guid? tenantId, CancellationToken ct = default);
}
