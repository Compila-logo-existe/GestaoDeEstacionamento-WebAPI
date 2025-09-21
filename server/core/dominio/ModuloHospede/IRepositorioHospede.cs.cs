using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloHospede;

public interface IRepositorioHospede : IRepositorio<Hospede>
{
    public Task<Hospede?> SelecionarRegistroPorCPFAsync(string cPF, Guid? tenantId, CancellationToken ct = default);
}
