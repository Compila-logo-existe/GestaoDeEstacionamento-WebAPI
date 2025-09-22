using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

public interface IRepositorioRegistroSaida : IRepositorio<RegistroSaida>
{
    public Task<RegistroSaida?> SelecionarPorTicketNumeroAsync(int numeroSequencial, Guid? tenantId, CancellationToken ct = default);
}
