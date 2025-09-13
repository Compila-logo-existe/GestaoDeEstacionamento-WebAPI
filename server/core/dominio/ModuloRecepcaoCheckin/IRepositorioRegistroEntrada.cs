using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

public interface IRepositorioRegistroEntrada : IRepositorio<RegistroEntrada>
{
    public Task<bool> ExisteAberturaPorPlacaAsync(string placa, Guid? usuarioId, CancellationToken cancellationToken = default);
}