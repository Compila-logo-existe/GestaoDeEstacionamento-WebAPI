using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloHospede;

public interface IRepositorioHospede : IRepositorio<Hospede>
{
    public Task<Hospede?> SelecionarRegistroPorCPFAsync(string cPF, Guid? usuarioId, CancellationToken ct = default);
}
