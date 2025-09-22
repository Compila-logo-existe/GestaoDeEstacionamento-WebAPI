using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de Unidade de ConviteRegistro (Domínio)")]
public class ConviteRegistroTestes
{
    private ConviteRegistro? conviteRegistro;

    [TestMethod]
    public void Deve_Criar_ConviteRegistro_Com_Sucesso()
    {
        // Arrange
        Guid usuarioEmissorIdEsperado = Guid.NewGuid();
        Guid tenantIdEsperado = Guid.NewGuid();
        string emailConvidadoEsperado = "convidado@teste.com";
        string nomeCargoEsperado = "Desenvolvedor Back-end";
        string tokenConviteEsperado = "token-de-convite-123";
        DateTime dataExpiracaoEsperada = DateTime.UtcNow.AddHours(2);

        // Act
        conviteRegistro = new ConviteRegistro(
            usuarioEmissorIdEsperado,
            tenantIdEsperado,
            emailConvidadoEsperado,
            nomeCargoEsperado,
            tokenConviteEsperado,
            dataExpiracaoEsperada
        );

        // Assert
        Assert.AreEqual(usuarioEmissorIdEsperado, conviteRegistro.UsuarioEmissorId);
        Assert.AreEqual(tenantIdEsperado, conviteRegistro.TenantId);
        Assert.AreEqual(emailConvidadoEsperado, conviteRegistro.EmailConvidado);
        Assert.AreEqual(nomeCargoEsperado, conviteRegistro.NomeCargo);
        Assert.AreEqual(tokenConviteEsperado, conviteRegistro.TokenConvite);
        Assert.AreEqual(dataExpiracaoEsperada, conviteRegistro.DataExpiracaoUtc);
        Assert.IsNull(conviteRegistro.UtilizadoEmUtc);
    }

    [TestMethod]
    public void Deve_Estar_Valido_Quando_Nao_Utilizado_E_Nao_Expirado()
    {
        // Arrange
        conviteRegistro = new ConviteRegistro
        {
            DataExpiracaoUtc = DateTime.UtcNow.AddMinutes(30),
        };

        // Act
        bool estaValido = conviteRegistro.EstaValidoAgora();

        // Assert
        Assert.IsTrue(estaValido);
    }

    [TestMethod]
    public void Nao_Deve_Estar_Valido_Quando_Expirado()
    {
        // Arrange
        conviteRegistro = new ConviteRegistro
        {
            DataExpiracaoUtc = DateTime.UtcNow.AddSeconds(-1),
        };
        conviteRegistro.MarcarComoUtilizado();

        // Act
        bool estaValido = conviteRegistro.EstaValidoAgora();

        // Assert
        Assert.IsFalse(estaValido);
    }

    [TestMethod]
    public void Nao_Deve_Estar_Valido_Quando_Ja_Utilizado()
    {
        // Arrange
        conviteRegistro = new ConviteRegistro
        {
            DataExpiracaoUtc = DateTime.UtcNow.AddHours(1)
        };

        // Act
        conviteRegistro.MarcarComoUtilizado();
        bool estaValido = conviteRegistro.EstaValidoAgora();

        // Assert
        Assert.IsNotNull(conviteRegistro.UtilizadoEmUtc);
        Assert.IsFalse(estaValido);
    }

    [TestMethod]
    public void Deve_Marcar_Com_Utilizado_Com_Sucesso()
    {
        // Arrange
        conviteRegistro = new ConviteRegistro
        {
            DataExpiracaoUtc = DateTime.UtcNow.AddMinutes(10)
        };

        // Act
        conviteRegistro.MarcarComoUtilizado();

        // Assert
        Assert.IsNotNull(conviteRegistro.UtilizadoEmUtc);
    }

    [TestMethod]
    public void Deve_Atualizar_Registro_Com_Sucesso()
    {
        // Arrange
        conviteRegistro = new ConviteRegistro(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "convidado@teste.com",
            "Cargo Antigo",
            "token-antigo",
            DateTime.UtcNow.AddHours(1)
        );

        ConviteRegistro conviteEditado = new()
        {
            NomeCargo = "Cargo Novo",
            DataExpiracaoUtc = DateTime.UtcNow.AddHours(3)
        };

        // Act
        conviteRegistro.AtualizarRegistro(conviteEditado);

        // Assert
        Assert.AreEqual("Cargo Novo", conviteRegistro.NomeCargo);
        Assert.AreEqual(conviteEditado.DataExpiracaoUtc, conviteRegistro.DataExpiracaoUtc);
    }

    [TestMethod]
    public void Nao_Deve_Atualizar_Registro()
    {
        // Arrange
        conviteRegistro = new ConviteRegistro(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "convidado@teste.com",
            "Cargo Antigo",
            "token-antigo",
            DateTime.UtcNow.AddHours(1)
        );

        ConviteRegistro conviteEditado = new()
        {
            NomeCargo = "Cargo Novo",
            DataExpiracaoUtc = DateTime.UtcNow.AddHours(3)
        };

        // Act
        conviteRegistro.AtualizarRegistro(null!);

        // Assert
        Assert.AreNotEqual("Cargo Novo", conviteRegistro.NomeCargo);
        Assert.AreNotEqual(conviteEditado.DataExpiracaoUtc, conviteRegistro.DataExpiracaoUtc);
    }
}
