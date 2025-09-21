using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;

public sealed class RepositorioRefreshToken(AppDbContext contexto)
    : RepositorioBaseORM<RefreshToken>(contexto), IRepositorioRefreshToken
{
    public async Task<RefreshToken?> SelecionarPorHashDoTokenAsync(string hashDoToken, CancellationToken ct)
    {
        return await registros.FirstOrDefaultAsync(x => x.HashDoToken == hashDoToken, ct);
    }

    public async Task<List<RefreshToken>> SelecionarAtivosDoUsuarioAsync(Guid usuarioAutenticadoId, CancellationToken ct)
    {
        return await registros
            .Where(x => x.UsuarioAutenticadoId == usuarioAutenticadoId
            && x.RevogadoEmUtc == null && x.ExpiraEmUtc >= DateTime.UtcNow)
            .ToListAsync(ct);
    }
}
