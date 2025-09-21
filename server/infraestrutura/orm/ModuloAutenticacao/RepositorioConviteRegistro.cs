using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;

public class RepositorioConviteRegistro(AppDbContext contexto)
    : RepositorioBaseORM<ConviteRegistro>(contexto), IRepositorioConvite
{
    public override async Task<ConviteRegistro?> SelecionarRegistroPorIdAsync(Guid idRegistro)
    {
        return await registros
            .Where(c => c.Id.Equals(idRegistro))
            .FirstOrDefaultAsync();
    }

    public async Task CriarAsync(ConviteRegistro convite, CancellationToken ct = default)
    {
        await registros.AddAsync(convite, ct);
    }

    public async Task<ConviteRegistro?> ObterAtivoPorTokenAsync(string tokenConvite, CancellationToken ct = default)
    {
        DateTime agoraUtc = DateTime.UtcNow;

        return await registros
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.TokenConvite == tokenConvite &&
                c.UtilizadoEmUtc == null &&
                c.DataExpiracaoUtc >= agoraUtc,
                ct
            );
    }

    public async Task MarcarComoUtilizadoAsync(Guid conviteId, CancellationToken ct = default)
    {
        ConviteRegistro? convite = await registros
            .FirstOrDefaultAsync(c => c.Id.Equals(conviteId), ct);

        if (convite is null)
            return;

        convite.MarcarComoUtilizado();
    }
}
