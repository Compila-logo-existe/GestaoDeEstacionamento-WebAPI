using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloRecepcaoCheckin;

public class RepositorioRegistroEntrada(AppDbContext contexto)
    : RepositorioBaseORM<RegistroEntrada>(contexto), IRepositorioRegistroEntrada
{
    public override async Task<List<RegistroEntrada>> SelecionarRegistrosAsync()
    {
        return await registros
            .Where(r => r.DataSaidaEmUtc == null)
            .Include(r => r.Hospede)
            .Include(r => r.Ticket)
            .Include(r => r.Veiculo)
            .ToListAsync();
    }

    public async Task<bool> ExisteAberturaPorPlacaAsync(string placa, Guid? usuarioId, CancellationToken cancellationToken = default)
    {
        return await registros
            .Where(r => r.UsuarioId.Equals(usuarioId) && r.DataSaidaEmUtc == null)
            .Include(r => r.Veiculo)
            .AnyAsync(r => r.Veiculo.Placa.Equals(placa), cancellationToken);
    }
}
