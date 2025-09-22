using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Testes.Unidades.Compartilhado;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de Unidade de RegistrarUsuarioCommandHandler")]
public class RegistrarUsuarioCommandHandlerTestes
{
    private RegistrarUsuarioCommandHandler handler;

    private const string fullNameCompletoPadrao = "Tester Segundo";
    private const string emailPadrao = "emailTeste@teste.com";
    private const string senhaPadrao = "Teste123!";
    private const string slug = "eTeste";
    private Tenant tenantPadrao = null!;

    private Mock<UserManager<Usuario>> userManagerMock = null!;
    private Mock<IRepositorioUsuarioTenant> repositorioUsuarioTenantMock = null!;
    private Mock<IRepositorioTenant> repositorioTenantMock = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<ITokenProvider> tokenProviderMock = null!;
    private Mock<IUnitOfWork> unitOfWorkMock = null!;
    private Mock<ILogger<RegistrarUsuarioCommandHandler>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        tenantPadrao = new(Guid.NewGuid(), "Empresa Teste", "00.000.000/0000-00",
            slug, null, DateTime.UtcNow);

        userManagerMock = new Mock<UserManager<Usuario>>(
            new Mock<IUserStore<Usuario>>().Object,
            null!, null!, null!, null!, null!, null!, null!, null!
        );

        repositorioUsuarioTenantMock = new Mock<IRepositorioUsuarioTenant>();
        repositorioTenantMock = new Mock<IRepositorioTenant>();
        tenantProviderMock = new Mock<ITenantProvider>();
        tokenProviderMock = new Mock<ITokenProvider>();
        unitOfWorkMock = new Mock<IUnitOfWork>();
        loggerMock = new Mock<ILogger<RegistrarUsuarioCommandHandler>>();

        tenantProviderMock
            .SetupGet(p => p.TenantId)
            .Returns(tenantPadrao.Id);

        handler = new RegistrarUsuarioCommandHandler(
            userManagerMock.Object,
            repositorioUsuarioTenantMock.Object,
            repositorioTenantMock.Object,
            tenantProviderMock.Object,
            tokenProviderMock.Object,
            unitOfWorkMock.Object,
            loggerMock.Object
        );
    }

    #region Testes Registro
    [TestMethod]
    public async Task Handle_Deve_Retornar_Sucesso_Quando_Registrar_Usuario_E_Gerar_Token()
    {
        // Arrange
        RegistrarUsuarioCommand command = new(fullNameCompletoPadrao, emailPadrao, senhaPadrao,
            senhaPadrao, tenantPadrao.Id, slug);

        Usuario usuarioEsperado = new()
        {
            FullName = command.NomeCompleto,
            UserName = command.Email,
            Email = command.Email
        };

        userManagerMock
            .Setup(u => u.CreateAsync(
                It.Is<Usuario>(usr =>
                    usr.FullName == command.NomeCompleto &&
                    usr.UserName == command.Email &&
                    usr.Email == command.Email),
                senhaPadrao
                ))
            .ReturnsAsync(IdentityResult.Success);

        repositorioTenantMock
            .Setup(r => r.ObterTenantIdPorSubdominioAsync(command.Slug!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenantPadrao.Id);

        repositorioTenantMock
            .Setup(r => r.ObterPorIdAsync(command.TenantId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenantPadrao);

        UsuarioAutenticado usuarioAutenticado =
            new(usuarioEsperado.Id, usuarioEsperado.FullName, usuarioEsperado.Email);

        AccessToken? accessTokenEsperado =
            new("token-simulacao", DateTime.UtcNow.AddMinutes(30), usuarioAutenticado);

        repositorioUsuarioTenantMock
            .Setup(r => r.CadastrarRegistroAsync(
                It.IsAny<VinculoUsuarioTenant>()));

        tokenProviderMock
            .Setup(t => t.GerarAccessToken(
                It.Is<Usuario>(usr =>
                    usr.FullName == command.NomeCompleto &&
                    usr.Email == command.Email),
                    tenantPadrao.Id
                ))
            .ReturnsAsync(accessTokenEsperado);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(
            It.Is<Usuario>(usr =>
                usr.FullName == command.NomeCompleto && usr.UserName == command.Email &&
                usr.Email == command.Email), senhaPadrao), Times.Once);

        repositorioTenantMock.Verify(r => r.ObterTenantIdPorSubdominioAsync(
            command.Slug!, It.IsAny<CancellationToken>()), Times.Once);

        repositorioTenantMock.Verify(r => r.ObterPorIdAsync(
            command.TenantId!.Value, It.IsAny<CancellationToken>()), Times.Once);

        repositorioUsuarioTenantMock.Verify(r => r.CadastrarRegistroAsync(
                It.IsAny<VinculoUsuarioTenant>()), Times.Once);

        tokenProviderMock.Verify(t => t.GerarAccessToken(
            It.Is<Usuario>(usr =>
                usr.FullName == command.NomeCompleto && usr.Email == command.Email),
            tenantPadrao.Id), Times.Once);

        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(accessTokenEsperado.Chave, resultado.Value.Chave);
        Assert.AreEqual(accessTokenEsperado.Expiracao, resultado.Value.Expiracao);
        Assert.AreEqual(accessTokenEsperado.UsuarioAutenticado.Id, resultado.Value.UsuarioAutenticado.Id);
        Assert.AreEqual(accessTokenEsperado.UsuarioAutenticado.NomeCompleto, resultado.Value.UsuarioAutenticado.NomeCompleto);
        Assert.AreEqual(accessTokenEsperado.UsuarioAutenticado.Email, resultado.Value.UsuarioAutenticado.Email);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Registrar_Usuario_Com_Nome_Duplicado()
    {
        // Arrange
        RegistrarUsuarioCommand command = new(fullNameCompletoPadrao, emailPadrao, senhaPadrao,
            senhaPadrao, tenantPadrao.Id, slug);

        IdentityError userDuplicateError = new()
        {
            Code = "DuplicateUserName",
            Description = "Já existe um usuário com esse nome."
        };

        userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao))
            .ReturnsAsync(IdentityResult.Failed(userDuplicateError));

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao), Times.Once);
        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);

        const string mensagemEsperada = MensagensErro.UsuarioJaExiste;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Requisição inválida", resultado.Errors[0].Message);
        Assert.IsTrue(mensagensDoResult.Count >= 1);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Registrar_Usuario_Com_Email_Duplicado()
    {
        // Arrange
        RegistrarUsuarioCommand command = new(fullNameCompletoPadrao, emailPadrao, senhaPadrao,
            senhaPadrao, tenantPadrao.Id, slug);

        IdentityError emailDuplicateError = new()
        {
            Code = "DuplicateEmail",
            Description = "Já existe um usuário com esse e-mail."
        };

        userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao))
            .ReturnsAsync(IdentityResult.Failed(emailDuplicateError));

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao), Times.Once);
        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);

        const string mensagemEsperada = MensagensErro.EmailJaExiste;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Requisição inválida", resultado.Errors[0].Message);
        Assert.IsTrue(mensagensDoResult.Count >= 1);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Registrar_Usuario_Com_Senha_Invalida()
    {
        // Arrange
        RegistrarUsuarioCommand command = new(fullNameCompletoPadrao, emailPadrao, senhaPadrao,
            senhaPadrao, tenantPadrao.Id, slug);

        IdentityError[] passwordErrors =
        {
                new() { Code = "PasswordTooShort", Description = "A senha é muito curta." },
                new() { Code = "PasswordRequiresNonAlphanumeric", Description = "A senha deve conter pelo menos um caractere especial." },
                new() { Code = "PasswordRequiresDigit", Description = "A senha deve conter pelo menos um número." },
                new() { Code = "PasswordRequiresUpper", Description = "A senha deve conter pelo menos uma letra maiúscula." },
                new() { Code = "PasswordRequiresLower", Description = "A senha deve conter pelo menos uma letra minúscula." }
            };

        userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao))
            .ReturnsAsync(IdentityResult.Failed(passwordErrors));

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao), Times.Once);
        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);

        ImmutableList<string> mensagensEsperadas =
        [
            MensagensErro.SenhaMuitoCurta,
                MensagensErro.SenhaRequerCaracterEspecial,
                MensagensErro.SenhaRequerNumero,
                MensagensErro.SenhaRequerMaiuscula,
                MensagensErro.SenhaRequerMinuscula
    ,
            ];
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Requisição inválida", resultado.Errors[0].Message);
        Assert.IsTrue(mensagensDoResult.Count >= 1);
        CollectionAssert.AreEquivalent(mensagensEsperadas, mensagensDoResult);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Registrar_Usuario_Com_Erro_Default()
    {
        // Arrange
        RegistrarUsuarioCommand command = new(fullNameCompletoPadrao, emailPadrao, senhaPadrao,
            senhaPadrao, tenantPadrao.Id, slug);

        IdentityError[] errosDesconhecidos =
        {
            new() { Code = "ErroInesperado", Description = "Erro inesperado no cadastro." }
            };

        userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao))
            .ReturnsAsync(IdentityResult.Failed(errosDesconhecidos));

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao), Times.Once);
        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);

        const string mensagemEsperada = MensagensErro.ErroInesperadoCadastro;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.IsTrue(mensagensDoResult.Count >= 1);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Registrar_Usuario_Com_Senhas_Diferentes()
    {
        // Arrange
        RegistrarUsuarioCommand command = new(fullNameCompletoPadrao, emailPadrao, senhaPadrao,
            "otrasenha", tenantPadrao.Id, slug);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao), Times.Never);
        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);

        const string mensagemEsperada = MensagensErro.ConfirmarSenha;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.IsTrue(mensagensDoResult.Count >= 1);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Registrar_Usuario_Com_Tenant_Nao_Inserido()
    {
        // Arrange
        RegistrarUsuarioCommand command = new(fullNameCompletoPadrao, emailPadrao, senhaPadrao,
            "otrasenha", tenantPadrao.Id, slug);

        tenantProviderMock
            .SetupGet(p => p.TenantId);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao), Times.Never);
        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);

        const string mensagemEsperada = MensagensErro.TenantNaoInformado;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.IsTrue(mensagensDoResult.Count >= 1);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Registrar_Usuario_Com_Tenant_Nao_Encontrado()
    {
        // Arrange
        RegistrarUsuarioCommand command = new(fullNameCompletoPadrao, emailPadrao, senhaPadrao,
            senhaPadrao, tenantPadrao.Id, slug);

        Usuario usuarioEsperado = new()
        {
            FullName = command.NomeCompleto,
            UserName = command.Email,
            Email = command.Email
        };

        userManagerMock
            .Setup(u => u.CreateAsync(
                It.Is<Usuario>(usr =>
                    usr.FullName == command.NomeCompleto &&
                    usr.UserName == command.Email &&
                    usr.Email == command.Email),
                senhaPadrao
                ))
            .ReturnsAsync(IdentityResult.Success);

        repositorioTenantMock
            .Setup(r => r.ObterTenantIdPorSubdominioAsync(command.Slug!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenantPadrao.Id);

        repositorioTenantMock
            .Setup(r => r.ObterPorIdAsync(command.TenantId!.Value, It.IsAny<CancellationToken>()));

        UsuarioAutenticado usuarioAutenticado =
            new(usuarioEsperado.Id, usuarioEsperado.FullName, usuarioEsperado.Email);

        AccessToken? accessTokenEsperado =
            new("token-simulacao", DateTime.UtcNow.AddMinutes(30), usuarioAutenticado);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(
            It.Is<Usuario>(usr =>
                usr.FullName == command.NomeCompleto && usr.UserName == command.Email &&
                usr.Email == command.Email), senhaPadrao), Times.Once);

        repositorioTenantMock.Verify(r => r.ObterTenantIdPorSubdominioAsync(
            command.Slug!, It.IsAny<CancellationToken>()), Times.Once);

        repositorioTenantMock.Verify(r => r.ObterPorIdAsync(
            command.TenantId!.Value, It.IsAny<CancellationToken>()), Times.Once);

        const string mensagemEsperada = MensagensErro.TenantNaoEncontrado;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.IsTrue(mensagensDoResult.Count >= 1);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Registrar_Usuario_E_Não_Gerar_Token()
    {
        // Arrange
        RegistrarUsuarioCommand command = new(fullNameCompletoPadrao, emailPadrao, senhaPadrao,
            senhaPadrao, tenantPadrao.Id, slug);

        Usuario usuarioEsperado = new()
        {
            FullName = command.NomeCompleto,
            UserName = command.Email,
            Email = command.Email
        };

        userManagerMock
            .Setup(u => u.CreateAsync(
                It.Is<Usuario>(usr =>
                    usr.FullName == command.NomeCompleto &&
                    usr.UserName == command.Email &&
                    usr.Email == command.Email),
                senhaPadrao
                ))
            .ReturnsAsync(IdentityResult.Success);

        repositorioTenantMock
            .Setup(r => r.ObterTenantIdPorSubdominioAsync(command.Slug!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenantPadrao.Id);

        repositorioTenantMock
            .Setup(r => r.ObterPorIdAsync(command.TenantId!.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenantPadrao);

        UsuarioAutenticado usuarioAutenticado =
            new(usuarioEsperado.Id, usuarioEsperado.FullName, usuarioEsperado.Email);

        AccessToken? accessTokenEsperado =
            new("token-simulacao", DateTime.UtcNow.AddMinutes(30), usuarioAutenticado);

        repositorioUsuarioTenantMock
            .Setup(r => r.CadastrarRegistroAsync(
                It.IsAny<VinculoUsuarioTenant>()));

        tokenProviderMock
            .Setup(t => t.GerarAccessToken(
                It.Is<Usuario>(usr =>
                    usr.FullName == command.NomeCompleto &&
                    usr.Email == command.Email),
                    tenantPadrao.Id
            ));

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(
            It.Is<Usuario>(usr =>
                usr.FullName == command.NomeCompleto && usr.UserName == command.Email &&
                usr.Email == command.Email), senhaPadrao), Times.Once);

        repositorioTenantMock.Verify(r => r.ObterTenantIdPorSubdominioAsync(
            command.Slug!, It.IsAny<CancellationToken>()), Times.Once);

        repositorioTenantMock.Verify(r => r.ObterPorIdAsync(
            command.TenantId!.Value, It.IsAny<CancellationToken>()), Times.Once);

        repositorioUsuarioTenantMock.Verify(r => r.CadastrarRegistroAsync(
                It.IsAny<VinculoUsuarioTenant>()), Times.Once);

        tokenProviderMock.Verify(t => t.GerarAccessToken(
            It.Is<Usuario>(usr =>
                usr.FullName == command.NomeCompleto && usr.Email == command.Email),
            tenantPadrao.Id), Times.Once);

        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);

        const string mensagemEsperada = MensagensErro.FalhaGerarToken;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.IsTrue(mensagensDoResult.Count >= 1);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Ocorrer_Excecao_Durante_Registro()
    {
        // Arrange
        RegistrarUsuarioCommand command = new(fullNameCompletoPadrao, emailPadrao, senhaPadrao,
            senhaPadrao, tenantPadrao.Id, slug);

        userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao))
            .ThrowsAsync(new Exception("Ocorreu um erro."));

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao), Times.Once);
        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);
        repositorioUsuarioTenantMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<VinculoUsuarioTenant>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);

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
