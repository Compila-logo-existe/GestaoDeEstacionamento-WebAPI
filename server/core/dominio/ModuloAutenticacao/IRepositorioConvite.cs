namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public interface IRepositorioConvite
{
    public Task CriarAsync(ConviteRegistro convite, CancellationToken ct = default);
    public Task<ConviteRegistro?> ObterAtivoPorTokenAsync(string tokenConvite, CancellationToken ct = default);
    public Task MarcarComoUtilizadoAsync(Guid conviteId, CancellationToken ct = default);
}
