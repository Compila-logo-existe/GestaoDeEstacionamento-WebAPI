namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public interface ITokenProvider
{
    AccessToken GerarAccessToken(Usuario usuario);
}
