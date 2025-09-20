namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public interface IRepositorioConvite
{
    public Task CriarAsync(ConviteRegistro convite, CancellationToken cancellationToken);
    public Task<ConviteRegistro?> ObterAtivoPorTokenAsync(string tokenConvite, CancellationToken cancellationToken);
    public Task MarcarComoUtilizadoAsync(Guid conviteId, CancellationToken cancellationToken);
}
