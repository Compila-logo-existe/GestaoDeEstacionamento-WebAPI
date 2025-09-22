using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de Unidade de RefreshToken (Domínio)")]
public class RefreshTokenTestes
{
    private RefreshToken? refreshToken;

    [TestMethod]
    public void Deve_Criar_RefreshToken_Com_Sucesso()
    {
        // Arrange
        Guid usuarioIdEsperado = Guid.NewGuid();
        string hashEsperado = "hash-token";
        DateTime criadoEmEsperado = DateTime.UtcNow.AddMinutes(-5);
        DateTime expiraEmEsperado = DateTime.UtcNow.AddHours(1);
        string ipEsperado = "127.0.0.1";
        string userAgentEsperado = "UnitTestAgent/1.0";

        // Act
        refreshToken = new RefreshToken
        {
            UsuarioAutenticadoId = usuarioIdEsperado,
            HashDoToken = hashEsperado,
            CriadoEmUtc = criadoEmEsperado,
            ExpiraEmUtc = expiraEmEsperado,
            EnderecoIpDeCriacao = ipEsperado,
            UserAgentDeCriacao = userAgentEsperado
        };

        // Assert
        Assert.AreEqual(usuarioIdEsperado, refreshToken.UsuarioAutenticadoId);
        Assert.AreEqual(hashEsperado, refreshToken.HashDoToken);
        Assert.AreEqual(criadoEmEsperado, refreshToken.CriadoEmUtc);
        Assert.AreEqual(expiraEmEsperado, refreshToken.ExpiraEmUtc);
        Assert.AreEqual(ipEsperado, refreshToken.EnderecoIpDeCriacao);
        Assert.AreEqual(userAgentEsperado, refreshToken.UserAgentDeCriacao);
    }

    [TestMethod]
    public void Deve_Vincular_Tenant_Com_Sucesso()
    {
        // Arrange
        refreshToken = new RefreshToken();
        Guid tenantIdEsperado = Guid.NewGuid();

        // Act
        refreshToken.VincularTenant(tenantIdEsperado);

        // Assert
        Assert.AreEqual(tenantIdEsperado, refreshToken.TenantId);
    }

    [TestMethod]
    public void Deve_Estar_Ativo_Quando_Nao_Revogado_E_Nao_Expirado()
    {
        // Arrange
        DateTime agora = DateTime.UtcNow;
        refreshToken = new RefreshToken
        {
            ExpiraEmUtc = agora.AddHours(2),
            RevogadoEmUtc = null
        };

        // Act
        bool ativo = refreshToken.EstaAtivo;

        // Assert
        Assert.IsTrue(ativo);
    }

    [TestMethod]
    public void Nao_Deve_Estar_Ativo_Quando_Revogado()
    {
        // Arrange
        DateTime agora = DateTime.UtcNow;
        refreshToken = new RefreshToken
        {
            ExpiraEmUtc = agora.AddHours(2),
            RevogadoEmUtc = agora
        };

        // Act
        bool ativo = refreshToken.EstaAtivo;

        // Assert
        Assert.IsFalse(ativo);
    }

    [TestMethod]
    public void Nao_Deve_Estar_Ativo_Quando_Expirado()
    {
        // Arrange
        DateTime agora = DateTime.UtcNow;
        refreshToken = new RefreshToken
        {
            ExpiraEmUtc = agora.AddSeconds(-1),
            RevogadoEmUtc = null
        };

        // Act
        bool ativo = refreshToken.EstaAtivo;

        // Assert
        Assert.IsFalse(ativo);
    }

    [TestMethod]
    public void Deve_Estar_Ativo_E_Com_Hash_Substituido()
    {
        // Arrange
        DateTime agora = DateTime.UtcNow;
        refreshToken = new RefreshToken
        {
            HashDoToken = "hash-token",
            ExpiraEmUtc = agora.AddSeconds(-1),
            RevogadoEmUtc = null
        };

        // Act
        refreshToken.SubstituidoPorHashDoToken = "hash-token";

        // Assert
        Assert.IsTrue(refreshToken.SubstituidoPorHashDoToken.Equals(refreshToken.HashDoToken));
    }

    [TestMethod]
    public void Deve_Atualizar_Registro_Com_Sucesso()
    {
        // Arrange
        refreshToken = new RefreshToken
        {
            EnderecoIpDeCriacao = "10.0.0.1",
            UserAgentDeCriacao = "OldAgent/0.1"
        };

        RefreshToken refreshTokenEditado = new()
        {
            EnderecoIpDeCriacao = "192.168.0.100",
            UserAgentDeCriacao = "NewAgent/2.0"
        };

        // Act
        refreshToken.AtualizarRegistro(refreshTokenEditado);

        // Assert
        Assert.AreEqual("192.168.0.100", refreshToken.EnderecoIpDeCriacao);
        Assert.AreEqual("NewAgent/2.0", refreshToken.UserAgentDeCriacao);
    }

    [TestMethod]
    public void Nao_Deve_Atualizar_Registro()
    {
        // Arrange
        refreshToken = new RefreshToken
        {
            EnderecoIpDeCriacao = "10.0.0.1",
            UserAgentDeCriacao = "OldAgent/0.1"
        };

        RefreshToken refreshTokenEditado = new()
        {
            EnderecoIpDeCriacao = "192.168.0.100",
            UserAgentDeCriacao = "NewAgent/2.0"
        };

        // Act
        refreshToken.AtualizarRegistro(null!);

        // Assert
        Assert.AreNotEqual("192.168.0.100", refreshToken.EnderecoIpDeCriacao);
        Assert.AreNotEqual("NewAgent/2.0", refreshToken.UserAgentDeCriacao);
    }
}
