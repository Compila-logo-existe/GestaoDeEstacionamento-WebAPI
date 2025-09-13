using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloHospede;

public class RepositorioHospede(AppDbContext contexto)
    : RepositorioBaseORM<Hospede>(contexto), IRepositorioHospede
{
    public async Task<Hospede?> SelecionarRegistroPorCPFAsync(string cPF, CancellationToken ct = default)
    {
        return await registros.Where(r => r.CPF.Equals(cPF)).FirstOrDefaultAsync(ct);
    }
}
