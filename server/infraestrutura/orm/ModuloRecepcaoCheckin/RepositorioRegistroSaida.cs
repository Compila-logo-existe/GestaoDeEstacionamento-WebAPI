using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloRecepcaoCheckin;

public class RepositorioRegistroSaida(AppDbContext contexto)
    : RepositorioBaseORM<RegistroSaida>(contexto), IRepositorioRegistroSaida
{
    public async Task<RegistroSaida?> SelecionarPorTicketNumeroAsync(int numeroSequencial, Guid? usuarioId, CancellationToken ct = default)
    {
        return await registros
            .Where(r => r.UsuarioId.Equals(usuarioId))
            .Include(r => r.Ticket)
            .Include(r => r.Veiculo)
            .FirstOrDefaultAsync(r => r.Ticket.NumeroSequencial.Equals(numeroSequencial), ct);
    }
}
