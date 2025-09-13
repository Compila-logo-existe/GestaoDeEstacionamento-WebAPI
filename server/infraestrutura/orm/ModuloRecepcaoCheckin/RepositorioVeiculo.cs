using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloRecepcaoCheckin;

public class RepositorioVeiculo(AppDbContext contexto)
    : RepositorioBaseORM<Veiculo>(contexto), IRepositorioVeiculo
{
    public async Task<Veiculo?> SelecionarRegistroPorPlacaAsync(string placa, Guid? usuarioId, CancellationToken ct = default)
    {
        return await registros.Where(v => v.Placa.Equals(placa) && v.UsuarioId.Equals(usuarioId))
            .FirstOrDefaultAsync(ct);
    }
}
