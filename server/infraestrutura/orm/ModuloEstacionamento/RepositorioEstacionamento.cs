using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloEstacionamento;

public class RepositorioEstacionamento(AppDbContext contexto)
    : RepositorioBaseORM<Estacionamento>(contexto), IRepositorioEstacionamento
{
    public override async Task<Estacionamento?> SelecionarRegistroPorIdAsync(Guid idRegistro)
    {
        return await registros.Where(v => v.Id.Equals(idRegistro))
            .Include(v => v.Vagas)
            .FirstOrDefaultAsync();
    }

    public async Task<Estacionamento?> SelecionarRegistroPorNome(string estacionamentoNome, Guid? tenantId, CancellationToken ct = default)
    {
        return await registros.Where(e => e.Nome.Equals(estacionamentoNome) && e.TenantId.Equals(tenantId))
            .Include(e => e.Vagas)
            .FirstOrDefaultAsync(ct);
    }
}
