namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public interface ITokenProvider
{
    Task<AccessToken> GerarAccessToken(
        Usuario usuario,
        Guid tenantId
    );
}
