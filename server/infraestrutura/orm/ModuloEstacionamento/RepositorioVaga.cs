using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloEstacionamento;
public class RepositorioVaga(AppDbContext contexto)
    : RepositorioBaseORM<Vaga>(contexto), IRepositorioVaga
{
    public override async Task<List<Vaga>> SelecionarRegistrosAsync()
    {
        return await registros.OrderBy(v => v.Zona).ThenBy(v => v.Numero).ToListAsync();
    }

    public override async Task<List<Vaga>> SelecionarRegistrosAsync(int quantidade)
    {
        return await registros.OrderBy(v => v.Zona).ThenBy(v => v.Numero)
            .Take(quantidade).ToListAsync();
    }

    public async Task<Vaga?> SelecionarPorVeiculoIdAsync(Guid veiculoId, CancellationToken ct = default)
    {
        return await registros.FirstOrDefaultAsync(v => v.VeiculoId.Equals(veiculoId), ct);
    }

    public async Task<Vaga?> SelecionarRegistroPorDadosAsync(int vagaNumero, ZonaEstacionamento? zona, Guid id, Guid? usuarioId, CancellationToken ct)
    {
        return await registros.FirstOrDefaultAsync(v => v.EstacionamentoId.Equals(id) && v.Numero.Equals(vagaNumero) && v.Zona == zona, ct);
    }

    public async Task<List<Vaga>> SelecionarRegistrosDoEstacionamentoAsync(Guid estacionamentoId, ZonaEstacionamento? zona, CancellationToken ct = default)
    {
        List<Vaga> vagasOcupadas = await registros
            .Where(v => v.EstacionamentoId.Equals(estacionamentoId) && v.Veiculo != null)
            .Include(v => v.Veiculo)
            .ToListAsync(ct);

        List<Vaga> outrasVagas = await registros
            .Where(v => v.EstacionamentoId.Equals(estacionamentoId) && v.Veiculo == null)
            .ToListAsync(ct);

        List<Vaga> vagas = vagasOcupadas.Concat(outrasVagas).ToList();

        if (zona.HasValue)
            vagas = vagas.Where(vaga => vaga.Zona == zona.Value).ToList();

        return vagas.OrderBy(keySelector: v => v.Zona).ThenBy(v => v.Numero).ToList();
    }

    public async Task<List<Vaga>> SelecionarRegistrosDoEstacionamentoAsync(int quantidade, Guid estacionamentoId, ZonaEstacionamento? zona, CancellationToken ct = default)
    {
        List<Vaga> vagasOcupadas = await registros
            .Where(v => v.EstacionamentoId.Equals(estacionamentoId) && v.Veiculo != null)
            .Include(v => v.Veiculo)
            .ToListAsync(ct);

        List<Vaga> outrasVagas = await registros
            .Where(v => v.EstacionamentoId.Equals(estacionamentoId) && v.Veiculo == null)
            .ToListAsync(ct);

        List<Vaga> vagas = vagasOcupadas.Concat(outrasVagas).ToList();

        if (zona.HasValue)
            vagas = vagas.Where(vaga => vaga.Zona == zona).ToList();

        return vagas.Take(quantidade).OrderBy(v => v.Zona).ThenBy(v => v.Numero).ToList();
    }
}
