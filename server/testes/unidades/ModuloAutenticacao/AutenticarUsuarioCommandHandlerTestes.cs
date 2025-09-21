using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.AspNetCore.Http;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de Unidade de AutenticarUsuarioCommandHandler")]
public class AutenticarUsuarioCommandHandlerTestes
{
    private AutenticarUsuarioCommandHandler handler;

    private const string fullNamePadrao = "Tester Segundo";
    private const string emailPadrao = "emailTeste@teste.com";
    private const string senhaPadrao = "Teste123!";
    private const string slug = "eTeste";
    private Tenant tenantPadrao = null!;

    private Mock<SignInManager<Usuario>> signInManagerMock = null!;
    private Mock<UserManager<Usuario>> userManagerMock = null!;
    private Mock<IRepositorioUsuarioTenant> repositorioUsuarioTenantMock = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<ITokenProvider> tokenProviderMock = null!;
    private Mock<ILogger<AutenticarUsuarioCommand>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        tenantPadrao = new(Guid.NewGuid(), "Empresa Teste", "00.000.000/0000-00",
            slug, null, DateTime.UtcNow);

        userManagerMock = new Mock<UserManager<Usuario>>(
            new Mock<IUserStore<Usuario>>().Object,
            null!, null!, null!, null!, null!, null!, null!, null!
        );

        signInManagerMock = new Mock<SignInManager<Usuario>>(
            userManagerMock.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<Usuario>>().Object,
            null!, null!, null!, null!
        );

        repositorioUsuarioTenantMock = new Mock<IRepositorioUsuarioTenant>();
        tenantProviderMock = new Mock<ITenantProvider>();
        tokenProviderMock = new Mock<ITokenProvider>();
        loggerMock = new Mock<ILogger<AutenticarUsuarioCommand>>();

        tenantProviderMock
            .SetupGet(p => p.TenantId)
            .Returns(tenantPadrao.Id);

        handler = new AutenticarUsuarioCommandHandler(
            signInManagerMock.Object,
            userManagerMock.Object,
            repositorioUsuarioTenantMock.Object,
            tenantProviderMock.Object,
            tokenProviderMock.Object,
            loggerMock.Object
        );
    }

    #region Testes Login
    [TestMethod]
    public async Task Handle_Deve_Retornar_Sucesso_Quando_Autenticar_Usuario()
    {
        // Arrange
        AutenticarUsuarioCommand command = new(emailPadrao, senhaPadrao, tenantPadrao.Id, slug);

        Usuario usuarioEsperado = new()
        {
            FullName = fullNamePadrao,
            UserName = command.Email,
            Email = command.Email
        };

        userManagerMock
            .Setup(u => u.FindByEmailAsync(command.Email))
            .ReturnsAsync(usuarioEsperado);

        userManagerMock
            .Setup(u => u.GetRolesAsync(usuarioEsperado))
            .ReturnsAsync(new List<string> { "User" });

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.TenantId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.Slug!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        signInManagerMock
            .Setup(p => p.PasswordSignInAsync(usuarioEsperado.UserName!, command.Senha, true, false))
            .ReturnsAsync(SignInResult.Success);

        UsuarioAutenticado usuarioAutenticado =
            new(usuarioEsperado.Id, usuarioEsperado.FullName, usuarioEsperado.Email);

        AccessToken accessTokenEsperado =
            new("token-simulacao", DateTime.UtcNow.AddMinutes(30), usuarioAutenticado);

        tokenProviderMock
            .Setup(t => t.GerarAccessToken(
                It.Is<Usuario>(usr =>
                    usr.FullName == usuarioEsperado.FullName &&
                    usr.Email == command.Email
                ),
                command.TenantId!.Value
            ))
            .ReturnsAsync(accessTokenEsperado);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.FindByEmailAsync(command.Email), Times.Once);
        userManagerMock.Verify(u => u.GetRolesAsync(usuarioEsperado), Times.Once);

        repositorioUsuarioTenantMock.Verify(r =>
            r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.TenantId!.Value, It.IsAny<CancellationToken>()), Times.Once);
        repositorioUsuarioTenantMock.Verify(r =>
            r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.Slug!, It.IsAny<CancellationToken>()), Times.Once);

        signInManagerMock.Verify(p =>
            p.PasswordSignInAsync(usuarioEsperado.UserName!, command.Senha, true, false), Times.Once);

        tokenProviderMock.Verify(t => t.GerarAccessToken(
            It.Is<Usuario>(usr =>
                usr.FullName == usuarioEsperado.FullName && usr.Email == usuarioEsperado.Email),
            command.TenantId!.Value), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(accessTokenEsperado.Chave, resultado.Value.Chave);
        Assert.AreEqual(accessTokenEsperado.Expiracao, resultado.Value.Expiracao);
        Assert.AreEqual(accessTokenEsperado.UsuarioAutenticado.Id, resultado.Value.UsuarioAutenticado.Id);
        Assert.AreEqual(accessTokenEsperado.UsuarioAutenticado.NomeCompleto, resultado.Value.UsuarioAutenticado.NomeCompleto);
        Assert.AreEqual(accessTokenEsperado.UsuarioAutenticado.Email, resultado.Value.UsuarioAutenticado.Email);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Autenticar_Usuario_Inexistente()
    {
        // Arrange
        AutenticarUsuarioCommand command = new(emailPadrao, senhaPadrao, tenantPadrao.Id, slug);

        Usuario usuarioEsperado = new()
        {
            FullName = fullNamePadrao,
            UserName = command.Email,
            Email = command.Email
        };

        userManagerMock
            .Setup(u => u.FindByEmailAsync(command.Email))
            .ReturnsAsync((Usuario?)null);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.FindByEmailAsync(command.Email), Times.Once);

        tokenProviderMock.Verify(t => t.GerarAccessToken(
            It.Is<Usuario>(usr =>
                usr.FullName == usuarioEsperado.FullName && usr.Email == usuarioEsperado.Email),
            It.IsAny<Guid>()), Times.Never);

        const string mensagemEsperada = MensagensErroAutenticacao.UsuarioInexistente;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual(1, mensagensDoResult.Count);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Autenticar_Usuario_Com_Login_Bloqueado()
    {
        // Arrange
        AutenticarUsuarioCommand command = new(emailPadrao, senhaPadrao, tenantPadrao.Id, slug);

        Usuario usuarioEsperado = new()
        {
            FullName = fullNamePadrao,
            UserName = command.Email,
            Email = command.Email
        };

        userManagerMock
            .Setup(u => u.FindByEmailAsync(command.Email))
            .ReturnsAsync(usuarioEsperado);

        userManagerMock
            .Setup(u => u.GetRolesAsync(usuarioEsperado))
            .ReturnsAsync(new List<string> { "User" });

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.TenantId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.Slug!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        signInManagerMock
            .Setup(p => p.PasswordSignInAsync(usuarioEsperado.UserName!, command.Senha, true, false))
            .ReturnsAsync(SignInResult.LockedOut);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.FindByEmailAsync(command.Email), Times.Once);

        tokenProviderMock.Verify(t => t.GerarAccessToken(
            It.IsAny<Usuario>(),
            It.IsAny<Guid>()), Times.Never);

        const string mensagemEsperada = MensagensErroAutenticacao.ContaBloqueada;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual(1, mensagensDoResult.Count);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Autenticar_Usuario_Com_Login_Nao_Permitido()
    {
        // Arrange
        AutenticarUsuarioCommand command = new(emailPadrao, senhaPadrao, tenantPadrao.Id, slug);

        Usuario usuarioEsperado = new()
        {
            FullName = fullNamePadrao,
            UserName = command.Email,
            Email = command.Email
        };

        userManagerMock
            .Setup(u => u.FindByEmailAsync(command.Email))
            .ReturnsAsync(usuarioEsperado);

        userManagerMock
            .Setup(u => u.GetRolesAsync(usuarioEsperado))
            .ReturnsAsync(new List<string> { "User" });

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.TenantId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.Slug!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        signInManagerMock
            .Setup(p => p.PasswordSignInAsync(usuarioEsperado.UserName!, command.Senha, true, false))
            .ReturnsAsync(SignInResult.NotAllowed);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.FindByEmailAsync(command.Email), Times.Once);

        tokenProviderMock.Verify(t => t.GerarAccessToken(
            It.IsAny<Usuario>(),
            It.IsAny<Guid>()), Times.Never);

        const string mensagemEsperada = MensagensErroAutenticacao.LoginNaoPermitido;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual(1, mensagensDoResult.Count);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Autenticar_Usuario_Com_Login_Requisitando_Auth_Dois_Fatores()
    {
        // Arrange
        AutenticarUsuarioCommand command = new(emailPadrao, senhaPadrao, tenantPadrao.Id, slug);

        Usuario usuarioEsperado = new()
        {
            FullName = fullNamePadrao,
            UserName = command.Email,
            Email = command.Email
        };

        userManagerMock
            .Setup(u => u.FindByEmailAsync(command.Email))
            .ReturnsAsync(usuarioEsperado);

        userManagerMock
            .Setup(u => u.GetRolesAsync(usuarioEsperado))
            .ReturnsAsync(new List<string> { "User" });

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.TenantId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.Slug!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        signInManagerMock
            .Setup(p => p.PasswordSignInAsync(usuarioEsperado.UserName!, command.Senha, true, false))
            .ReturnsAsync(SignInResult.TwoFactorRequired);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.FindByEmailAsync(command.Email), Times.Once);

        tokenProviderMock.Verify(t => t.GerarAccessToken(
            It.IsAny<Usuario>(),
            It.IsAny<Guid>()), Times.Never);

        const string mensagemEsperada = MensagensErroAutenticacao.RequerAutenticacaoDoisFatores;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual(1, mensagensDoResult.Count);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Autenticar_Usuario_Com_Login_Inválido()
    {
        // Arrange
        AutenticarUsuarioCommand command = new(emailPadrao, senhaPadrao, tenantPadrao.Id, slug);

        Usuario usuarioEsperado = new()
        {
            FullName = fullNamePadrao,
            UserName = command.Email,
            Email = command.Email
        };

        userManagerMock
            .Setup(u => u.FindByEmailAsync(command.Email))
            .ReturnsAsync(usuarioEsperado);

        userManagerMock
            .Setup(u => u.GetRolesAsync(usuarioEsperado))
            .ReturnsAsync(new List<string> { "User" });

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.TenantId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.Slug!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        signInManagerMock
            .Setup(p => p.PasswordSignInAsync(usuarioEsperado.UserName!, command.Senha, true, false))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.FindByEmailAsync(command.Email), Times.Once);

        tokenProviderMock.Verify(t => t.GerarAccessToken(
            It.IsAny<Usuario>(),
            It.IsAny<Guid>()), Times.Never);

        const string mensagemEsperada = MensagensErroAutenticacao.DadosInvalidos;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual(1, mensagensDoResult.Count);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Autenticar_Usuario_Com_Tenant_Nao_Inserido()
    {
        // Arrange
        AutenticarUsuarioCommand command = new(emailPadrao, senhaPadrao, tenantPadrao.Id, slug);

        tenantProviderMock
            .SetupGet(p => p.TenantId);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);

        const string mensagemEsperada = MensagensErroAutenticacao.TenantNaoInformado;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual(1, mensagensDoResult.Count);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Usuario_Nao_Pertence_A_Empresa()
    {
        // Arrange
        AutenticarUsuarioCommand command = new(emailPadrao, senhaPadrao, tenantPadrao.Id, slug);

        Usuario usuarioEsperado = new()
        {
            FullName = fullNamePadrao,
            UserName = command.Email,
            Email = command.Email
        };

        userManagerMock
            .Setup(u => u.FindByEmailAsync(command.Email))
            .ReturnsAsync(usuarioEsperado);

        userManagerMock
            .Setup(u => u.GetRolesAsync(usuarioEsperado))
            .ReturnsAsync(new List<string> { "User" });

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.TenantId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.Slug!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.FindByEmailAsync(command.Email), Times.Once);
        userManagerMock.Verify(u => u.GetRolesAsync(usuarioEsperado), Times.Once);

        repositorioUsuarioTenantMock.Verify(r =>
            r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.TenantId!.Value, It.IsAny<CancellationToken>()), Times.Once);
        repositorioUsuarioTenantMock.Verify(r =>
            r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.Slug!, It.IsAny<CancellationToken>()), Times.Once);

        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);

        const string mensagemEsperada = "Você não pertence a esta empresa. Confira o Tenant e o Slug.";
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual(1, mensagensDoResult.Count);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Ocorrer_Excecao_Durante_Autenticacao()
    {
        // Arrange
        AutenticarUsuarioCommand command = new(emailPadrao, senhaPadrao, tenantPadrao.Id, slug);

        Usuario usuarioEsperado = new()
        {
            FullName = fullNamePadrao,
            UserName = command.Email,
            Email = command.Email
        };

        userManagerMock
            .Setup(u => u.FindByEmailAsync(command.Email))
            .ReturnsAsync(usuarioEsperado);

        userManagerMock
            .Setup(u => u.GetRolesAsync(usuarioEsperado))
            .ReturnsAsync(new List<string> { "User" });

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.TenantId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.Slug!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        signInManagerMock
            .Setup(p => p.PasswordSignInAsync(usuarioEsperado.UserName!, command.Senha, true, false))
            .ThrowsAsync(new Exception("Ocorreu um erro."));

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        userManagerMock.Verify(u => u.FindByEmailAsync(command.Email), Times.Once);
        userManagerMock.Verify(u => u.GetRolesAsync(usuarioEsperado), Times.Once);
        repositorioUsuarioTenantMock.Verify(r =>
            r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.TenantId!.Value, It.IsAny<CancellationToken>()), Times.Once);
        repositorioUsuarioTenantMock.Verify(r =>
            r.UsuarioPertenceAoTenantAsync(usuarioEsperado.Id, command.Slug!, It.IsAny<CancellationToken>()), Times.Once);
        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);

        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.IsNotNull(mensagensDoResult);
        Assert.IsTrue(mensagensDoResult.Count >= 1);
    }

    #endregion
}
