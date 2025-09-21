using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloRecepcaoCheckin;

public class RepositorioVeiculo(AppDbContext contexto)
    : RepositorioBaseORM<Veiculo>(contexto), IRepositorioVeiculo
{
    public override async Task<Veiculo?> SelecionarRegistroPorIdAsync(Guid idRegistro)
    {
        return await registros.Where(v => v.Id.Equals(idRegistro))
            .Include(v => v.Hospede)
            .Include(v => v.Vaga)
            .ThenInclude(v => v.Estacionamento)
            .FirstOrDefaultAsync();
    }
    public async Task<Veiculo?> SelecionarRegistroPorPlacaAsync(string placa, Guid? tenantId, CancellationToken ct = default)
    {
        return await registros.Where(v => v.Placa.Equals(placa) && v.TenantId.Equals(tenantId))
            .Include(v => v.Hospede)
            .Include(v => v.RegistrosEntrada)
            .Include(v => v.Vaga)
            .ThenInclude(v => v.Estacionamento)
            .FirstOrDefaultAsync(ct);
    }
}
