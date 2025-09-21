using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public interface IRepositorioRefreshToken : IRepositorio<RefreshToken>
{
    public Task<RefreshToken?> SelecionarPorHashDoTokenAsync(string hashDoToken, CancellationToken ct = default);
    public Task<List<RefreshToken>> SelecionarAtivosDoUsuarioAsync(Guid usuarioAutenticadoId, CancellationToken ct = default);
}
