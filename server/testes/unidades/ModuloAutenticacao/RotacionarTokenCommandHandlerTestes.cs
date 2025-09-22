using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de Unidade de RotacionarTokenCommandHandler")]
public class RotacionarTokenCommandHandlerTestes
{
    private RotacionarTokenCommandHandler handler = null!;

    private const string refreshTokenOriginal = "refresh-token-original";
    private const string refreshTokenNovo = "refresh-token-novo";
    private const string nomeCompletoPadrao = "Tester Segundo";
    private const string emailPadrao = "tester@exemplo.com";

    private readonly Guid tenantIdPadrao = Guid.NewGuid();

    private Mock<IUnitOfWork> unitOfWorkMock = null!;
    private Mock<ITokenProvider> tokenProviderMock = null!;
    private Mock<IRefreshTokenProvider> refreshTokenProviderMock = null!;
    private Mock<ILogger<RotacionarTokenCommandHandler>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        unitOfWorkMock = new Mock<IUnitOfWork>();
        tokenProviderMock = new Mock<ITokenProvider>();
        refreshTokenProviderMock = new Mock<IRefreshTokenProvider>();
        loggerMock = new Mock<ILogger<RotacionarTokenCommandHandler>>();

        handler = new RotacionarTokenCommandHandler(
            unitOfWorkMock.Object,
            tokenProviderMock.Object,
            refreshTokenProviderMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Sucesso_Quando_Rotacionar_Token()
    {
        // Arrange
        RotacionarTokenCommand command = new(refreshTokenOriginal);

        Usuario usuarioResultadoRotacao = new()
        {
            Id = Guid.NewGuid(),
            FullName = nomeCompletoPadrao,
            Email = emailPadrao,
            UserName = emailPadrao,
            AccessTokenVersionId = Guid.NewGuid()
        };
        Guid versaoAnteriorAccessToken = usuarioResultadoRotacao.AccessTokenVersionId;

        refreshTokenProviderMock
            .Setup(p => p.RotacionarRefreshTokenAsync(
                It.Is<string>(s => s == refreshTokenOriginal),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(
                (Usuario: usuarioResultadoRotacao, TenantId: tenantIdPadrao, NovoRefreshToken: refreshTokenNovo)));

        UsuarioAutenticado usuarioAutenticado =
            new(usuarioResultadoRotacao.Id, usuarioResultadoRotacao.FullName, usuarioResultadoRotacao.Email);

        AccessToken accessTokenEsperado =
            new("token-simulacao", DateTime.UtcNow.AddMinutes(30), usuarioAutenticado);

        tokenProviderMock
            .Setup(t => t.GerarAccessToken(
                It.Is<Usuario>(u => u == usuarioResultadoRotacao && u.AccessTokenVersionId != versaoAnteriorAccessToken),
                It.Is<Guid>(g => g == tenantIdPadrao)))
            .ReturnsAsync(accessTokenEsperado);

        unitOfWorkMock
            .Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        Result<(AccessToken, string)> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        refreshTokenProviderMock.Verify(p =>
            p.RotacionarRefreshTokenAsync(refreshTokenOriginal, It.IsAny<CancellationToken>()), Times.Once);
        tokenProviderMock.Verify(t =>
            t.GerarAccessToken(It.Is<Usuario>(u => u == usuarioResultadoRotacao && u.AccessTokenVersionId != versaoAnteriorAccessToken),
                               tenantIdPadrao), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(accessTokenEsperado.Chave, resultado.Value.Item1.Chave);
        Assert.AreEqual(accessTokenEsperado.Expiracao, resultado.Value.Item1.Expiracao);
        Assert.AreEqual(accessTokenEsperado.UsuarioAutenticado.Id, resultado.Value.Item1.UsuarioAutenticado.Id);
        Assert.AreEqual(accessTokenEsperado.UsuarioAutenticado.NomeCompleto, resultado.Value.Item1.UsuarioAutenticado.NomeCompleto);
        Assert.AreEqual(accessTokenEsperado.UsuarioAutenticado.Email, resultado.Value.Item1.UsuarioAutenticado.Email);
        Assert.AreEqual(refreshTokenNovo, resultado.Value.Item2);
        Assert.AreNotEqual(versaoAnteriorAccessToken, usuarioResultadoRotacao.AccessTokenVersionId);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Rotacao_De_RefreshToken_Falhar()
    {
        // Arrange
        RotacionarTokenCommand command = new(refreshTokenOriginal);

        refreshTokenProviderMock
            .Setup(p => p.RotacionarRefreshTokenAsync(
                It.Is<string>(s => s == refreshTokenOriginal),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("refresh inválido"));

        // Act
        Result<(AccessToken, string)> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert 
        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_E_Logar_Quando_Ocorrer_Excecao_Ao_Gerar_AccessToken()
    {
        // Arrange
        RotacionarTokenCommand command = new(refreshTokenOriginal);

        Usuario usuarioResultadoRotacao = new()
        {
            Id = Guid.NewGuid(),
            FullName = nomeCompletoPadrao,
            Email = emailPadrao,
            UserName = emailPadrao,
            AccessTokenVersionId = Guid.NewGuid()
        };
        Guid versaoAnteriorAccessToken = usuarioResultadoRotacao.AccessTokenVersionId;

        refreshTokenProviderMock
            .Setup(p => p.RotacionarRefreshTokenAsync(
                It.Is<string>(s => s == refreshTokenOriginal),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(
                (Usuario: usuarioResultadoRotacao, TenantId: tenantIdPadrao, NovoRefreshToken: refreshTokenNovo)));

        tokenProviderMock
            .Setup(t => t.GerarAccessToken(
                It.Is<Usuario>(u => u == usuarioResultadoRotacao && u.AccessTokenVersionId != versaoAnteriorAccessToken),
                It.Is<Guid>(g => g == tenantIdPadrao)))
            .ThrowsAsync(new Exception("erro ao gerar token"));

        // Act
        Result<(AccessToken, string)> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_E_Logar_Quando_Ocorrer_Excecao_No_Commit()
    {
        // Arrange
        RotacionarTokenCommand command = new(refreshTokenOriginal);

        Usuario usuarioResultadoRotacao = new()
        {
            Id = Guid.NewGuid(),
            FullName = nomeCompletoPadrao,
            Email = emailPadrao,
            UserName = emailPadrao,
            AccessTokenVersionId = Guid.NewGuid()
        };
        Guid versaoAnteriorAccessToken = usuarioResultadoRotacao.AccessTokenVersionId;

        refreshTokenProviderMock
            .Setup(p => p.RotacionarRefreshTokenAsync(
                It.Is<string>(s => s == refreshTokenOriginal),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(
                (Usuario: usuarioResultadoRotacao, TenantId: tenantIdPadrao, NovoRefreshToken: refreshTokenNovo)));

        UsuarioAutenticado usuarioAutenticado =
            new(usuarioResultadoRotacao.Id, usuarioResultadoRotacao.FullName, usuarioResultadoRotacao.Email);

        AccessToken accessTokenEsperado =
            new("token-simulacao", DateTime.UtcNow.AddMinutes(30), usuarioAutenticado);

        tokenProviderMock
            .Setup(t => t.GerarAccessToken(
                It.Is<Usuario>(u => u == usuarioResultadoRotacao && u.AccessTokenVersionId != versaoAnteriorAccessToken),
                It.Is<Guid>(g => g == tenantIdPadrao)))
            .ReturnsAsync(accessTokenEsperado);

        unitOfWorkMock
            .Setup(u => u.CommitAsync())
            .ThrowsAsync(new Exception("ocorreu um erro."));

        // Act
        Result<(AccessToken, string)> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        Assert.IsTrue(resultado.IsFailed);
    }
}
