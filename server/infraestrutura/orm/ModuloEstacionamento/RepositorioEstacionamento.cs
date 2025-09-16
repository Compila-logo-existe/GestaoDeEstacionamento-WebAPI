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

    public async Task<Estacionamento?> SelecionarRegistroPorNome(string estacionamentoNome, Guid? usuarioId, CancellationToken ct)
    {
        return await registros.Where(e => e.Nome.Equals(estacionamentoNome) && e.UsuarioId.Equals(usuarioId))
            .Include(e => e.Vagas)
            .FirstOrDefaultAsync(ct);
    }
}
