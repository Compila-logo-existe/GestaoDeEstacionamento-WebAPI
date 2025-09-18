using GestaoDeEstacionamento.Core.Dominio.ModuloFaturamento;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloFaturamento;

public class RepositorioFaturamento(AppDbContext contexto)
    : RepositorioBaseORM<Faturamento>(contexto), IRepositorioFaturamento
{
    public async Task<List<Faturamento>> SelecionarPorPeriodoAsync(
        DateTime dataInicialEmUtc, DateTime dataFinalEmUtc, Guid? usuarioId, CancellationToken ct = default)
    {
        return await registros
            .Where(f => f.UsuarioId.Equals(usuarioId)
                     && f.DataEntradaEmUtc >= dataInicialEmUtc
                     && f.DataEntradaEmUtc <= dataFinalEmUtc)
            .Include(f => f.RegistroEntrada)
            .Include(f => f.RegistroSaida)
                .ThenInclude(r => r.Ticket)
                .ThenInclude(t => t.RegistroEntrada)
            .Include(f => f.RegistroSaida)
                .ThenInclude(r => r.Veiculo)
            .OrderBy(f => f.DataEntradaEmUtc)
            .ToListAsync(ct);
    }
}
