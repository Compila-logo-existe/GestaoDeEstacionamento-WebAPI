namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public interface IRefreshTokenCookieService
{
    public void Gravar(string valorRefreshTokenEmTextoPlano, int quantidadeDiasDeValidade, bool ambienteCrossSite);
    public string? Ler();
    public void Remover();
}
