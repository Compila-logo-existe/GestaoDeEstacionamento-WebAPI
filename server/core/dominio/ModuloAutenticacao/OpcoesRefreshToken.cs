namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public class OpcoesRefreshToken
{
    public int QuantidadeDiasDeValidade { get; set; } = 7;
    public string SegredoPepper { get; set; } = string.Empty;
    public string NomeDoCookie { get; set; } = "rt";
}
