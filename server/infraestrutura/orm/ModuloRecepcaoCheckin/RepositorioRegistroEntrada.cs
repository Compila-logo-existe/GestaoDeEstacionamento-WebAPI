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
            .Include(r => r.Hospede)
            .Include(r => r.Ticket)
            .Include(r => r.Veiculo)
            .OrderByDescending(r => r.DataEntradaEmUtc)
            .ToListAsync();
    }

    public override async Task<List<RegistroEntrada>> SelecionarRegistrosAsync(int quantidade)
    {
        return await registros
            .Take(quantidade)
            .Include(r => r.Hospede)
            .Include(r => r.Ticket)
            .Include(r => r.Veiculo)
            .OrderByDescending(r => r.DataEntradaEmUtc)
            .ToListAsync();
    }

    // criar selecao de registros em geral e do veiculo, filtrando por abertos e fechados
    public async Task<List<RegistroEntrada>> SelecionarRegistrosDoVeiculoAsync(Guid veiculoId, CancellationToken ct = default)
    {
        return await registros
            .Where(r => r.VeiculoId.Equals(veiculoId))
            .Include(r => r.Hospede)
            .Include(r => r.Ticket)
            .Include(r => r.Veiculo)
            .OrderByDescending(r => r.DataEntradaEmUtc)
            .ToListAsync(ct);
    }

    public async Task<List<RegistroEntrada>> SelecionarRegistrosDoVeiculoAsync(int quantidade, Guid veiculoId, CancellationToken ct = default)
    {
        return await registros
            .Take(quantidade)
            .Where(r => r.VeiculoId.Equals(veiculoId))
            .Include(r => r.Hospede)
            .Include(r => r.Ticket)
            .Include(r => r.Veiculo)
            .OrderByDescending(r => r.DataEntradaEmUtc)
            .ToListAsync(ct);
    }

    public async Task<RegistroEntrada?> SelecionarAberturaPorNumeroDoTicketAsync(int numeroSequencial, Guid? usuarioId, CancellationToken ct = default)
    {
        return await registros
            .Where(r => r.UsuarioId.Equals(usuarioId) && r.Ticket.RegistroSaida == null && r.Ticket.NumeroSequencial.Equals(numeroSequencial))
            .Include(r => r.Hospede)
            .Include(r => r.Veiculo)
            .Include(r => r.Ticket)
            .OrderByDescending(r => r.DataEntradaEmUtc)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<RegistroEntrada?> SelecionarAberturaPorPlacaAsync(string placa, Guid? usuarioId, CancellationToken ct = default)
    {
        return await registros
            .Where(r => r.UsuarioId.Equals(usuarioId) && r.Ticket.RegistroSaida == null)
            .Include(r => r.Hospede)
            .Include(r => r.Veiculo)
            .Include(r => r.Ticket)
            .OrderByDescending(r => r.DataEntradaEmUtc)
            .FirstOrDefaultAsync(r => r.Veiculo.Placa == placa, ct);
    }

    public async Task<bool> ExisteAberturaPorPlacaAsync(string placa, Guid? usuarioId, CancellationToken ct = default)
    {
        return await registros
            .Where(r => r.UsuarioId.Equals(usuarioId) && r.Ticket.RegistroSaida == null)
            .Include(r => r.Veiculo)
            .AnyAsync(r => r.Veiculo.Placa.Equals(placa), ct);
    }

    public async Task<bool> ExisteAberturaPorNumeroDoTicketAsync(int numeroSequencial, Guid? usuarioId, CancellationToken ct = default)
    {
        return await registros
            .Where(r => r.UsuarioId.Equals(usuarioId) && r.Ticket.RegistroSaida == null)
            .Include(r => r.Ticket)
            .AnyAsync(r => r.Ticket.NumeroSequencial.Equals(numeroSequencial), ct);
    }
}
