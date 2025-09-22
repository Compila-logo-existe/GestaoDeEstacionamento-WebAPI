using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

public interface IRepositorioRegistroEntrada : IRepositorio<RegistroEntrada>
{
    public Task<bool> ExisteAberturaPorPlacaAsync(string placa, Guid? tenantId, CancellationToken ct = default);
    public Task<bool> ExisteAberturaPorNumeroDoTicketAsync(int numeroSequencial, Guid? tenantId, CancellationToken ct = default);
    public Task<List<RegistroEntrada>> SelecionarRegistrosDoVeiculoAsync(Guid veiculoId, CancellationToken ct = default);
    public Task<List<RegistroEntrada>> SelecionarRegistrosDoVeiculoAsync(int quantidade, Guid veiculoId, CancellationToken ct = default);
    public Task<RegistroEntrada?> SelecionarAberturaPorNumeroDoTicketAsync(int numeroSequencial, Guid? tenantId, CancellationToken ct = default);
    public Task<RegistroEntrada?> SelecionarAberturaPorPlacaAsync(string placa, Guid? tenantId, CancellationToken ct = default);
}