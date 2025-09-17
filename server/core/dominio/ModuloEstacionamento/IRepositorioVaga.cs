using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;

public interface IRepositorioVaga : IRepositorio<Vaga>
{
    public Task<Vaga?> SelecionarPorVeiculoIdAsync(Guid veiculoId, CancellationToken ct = default);
    public Task<Vaga?> SelecionarRegistroPorDadosAsync(int vagaNumero, ZonaEstacionamento? zona, Guid id, Guid? usuarioId, CancellationToken ct = default);
    public Task<List<Vaga>> SelecionarRegistrosDoEstacionamentoAsync(Guid estacionamentoId, ZonaEstacionamento? zona, CancellationToken ct = default);
    public Task<List<Vaga>> SelecionarRegistrosDoEstacionamentoAsync(int quantidade, Guid estacionamentoId, ZonaEstacionamento? zona, CancellationToken ct = default);
}
