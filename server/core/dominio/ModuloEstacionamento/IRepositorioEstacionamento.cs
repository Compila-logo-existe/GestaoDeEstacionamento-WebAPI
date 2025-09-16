using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;

public interface IRepositorioEstacionamento : IRepositorio<Estacionamento>
{
    public Task<Estacionamento?> SelecionarRegistroPorNome(string estacionamentoNome, Guid? usuarioId, CancellationToken ct = default);
}
