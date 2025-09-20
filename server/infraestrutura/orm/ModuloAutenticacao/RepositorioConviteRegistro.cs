using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Infraestrutura.ORM.Compartilhado;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeEstacionamento.Infraestrutura.ORM.ModuloAutenticacao;

public class RepositorioConviteRegistro(AppDbContext contexto)
    : RepositorioBaseORM<ConviteRegistro>(contexto), IConviteRepositorio
{
    public override async Task<ConviteRegistro?> SelecionarRegistroPorIdAsync(Guid idRegistro)
    {
        return await registros
            .Where(c => c.Id.Equals(idRegistro))
            .FirstOrDefaultAsync();
    }

    public async Task CriarAsync(ConviteRegistro convite, CancellationToken cancellationToken)
    {
        await registros.AddAsync(convite, cancellationToken);
    }

    public async Task<ConviteRegistro?> ObterAtivoPorTokenAsync(string tokenConvite, CancellationToken cancellationToken)
    {
        DateTime agoraUtc = DateTime.UtcNow;

        return await registros
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.TokenConvite == tokenConvite &&
                c.UtilizadoEmUtc == null &&
                c.DataExpiracaoUtc >= agoraUtc,
                cancellationToken
            );
    }

    public async Task MarcarComoUtilizadoAsync(Guid conviteId, CancellationToken cancellationToken)
    {
        ConviteRegistro? convite = await registros
            .FirstOrDefaultAsync(c => c.Id.Equals(conviteId), cancellationToken);

        if (convite is null)
            return;

        convite.MarcarComoUtilizado();
    }
}
