namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public interface ITokenProvider
{
    public Task<AccessToken> GerarAccessToken(Usuario usuario, Guid tenantId);
}
