using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de Unidade de OpcoesRefreshToken (Domínio)")]
public class OpcoesRefreshTokenTestes
{
    private OpcoesRefreshToken? opcoes;

    [TestMethod]
    public void Deve_Criar_Com_Valores_Padrao()
    {
        // Act
        opcoes = new OpcoesRefreshToken();

        // Assert
        Assert.AreEqual(7, opcoes.QuantidadeDiasDeValidade);
        Assert.AreEqual(string.Empty, opcoes.SegredoPepper);
        Assert.AreEqual("rt", opcoes.NomeDoCookie);
    }

    [TestMethod]
    public void Deve_Alterar_Propriedades_Com_Sucesso()
    {
        // Arrange
        opcoes = new OpcoesRefreshToken();

        const int quantidadeDiasDeValidadeEsperada = 14;
        const string segredoPepperEsperado = "pepper-super-secreto";
        const string nomeDoCookieEsperado = "refresh_token";

        // Act
        opcoes.QuantidadeDiasDeValidade = quantidadeDiasDeValidadeEsperada;
        opcoes.SegredoPepper = segredoPepperEsperado;
        opcoes.NomeDoCookie = nomeDoCookieEsperado;

        // Assert
        Assert.AreEqual(quantidadeDiasDeValidadeEsperada, opcoes.QuantidadeDiasDeValidade);
        Assert.AreEqual(segredoPepperEsperado, opcoes.SegredoPepper);
        Assert.AreEqual(nomeDoCookieEsperado, opcoes.NomeDoCookie);
    }
}
