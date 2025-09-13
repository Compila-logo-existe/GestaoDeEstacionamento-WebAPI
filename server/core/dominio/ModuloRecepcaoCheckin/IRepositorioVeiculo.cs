using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

public interface IRepositorioVeiculo : IRepositorio<Veiculo>
{
    public Task<Veiculo?> SelecionarRegistroPorPlacaAsync(string placa, Guid? usuarioId, CancellationToken ct = default);
}
