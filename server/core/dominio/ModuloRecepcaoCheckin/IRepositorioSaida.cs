using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

public interface IRepositorioRegistroSaida : IRepositorio<RegistroSaida>
{
    Task<RegistroSaida?> SelecionarPorTicketNumeroAsync(int numeroSequencial, Guid? usuarioId, CancellationToken ct = default);
}
