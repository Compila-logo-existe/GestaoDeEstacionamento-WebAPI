using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de Unidade de RegistrarUsuarioCommandHandler")]
public class RegistrarUsuarioCommandHandlerTestes
{
    private RegistrarUsuarioCommandHandler handler;

    private const string fullNameCompletoPadrao = "Tester Segundo";
    private const string emailPadrao = "emailTeste@teste.com";
    private const string senhaPadrao = "Teste123!";

    private Mock<UserManager<Usuario>> userManagerMock;
    private Mock<ITokenProvider> tokenProviderMock;
    private Mock<ILogger<UserManager<Usuario>>> loggerMock;


    [TestInitialize]
    public void Setup()
    {
        loggerMock = new Mock<ILogger<UserManager<Usuario>>>();

        userManagerMock = new Mock<UserManager<Usuario>>(
            new Mock<IUserStore<Usuario>>().Object, null!, null!, null!,
            null!, null!, null!, null!, loggerMock.Object
        );

        tokenProviderMock = new Mock<ITokenProvider>();

        handler = new RegistrarUsuarioCommandHandler(
                userManagerMock.Object,
                tokenProviderMock.Object
                );
    }

    #region Testes Registro
    [TestMethod]
    public async Task Handle_Deve_Retornar_Sucesso_Quando_Registrar_Usuario_E_Gerar_Token()
    {
        // Arrange
        RegistrarUsuarioCommand registrarUsuarioCommand =
            new(fullNameCompletoPadrao, emailPadrao, senhaPadrao, senhaPadrao);

        Usuario usuarioEsperado = new()
        {
            FullName = registrarUsuarioCommand.NomeCompleto,
            UserName = registrarUsuarioCommand.Email,
            Email = registrarUsuarioCommand.Email
        };

        userManagerMock
            .Setup(u => u.CreateAsync(
                It.Is<Usuario>(usr =>
                    usr.FullName == registrarUsuarioCommand.NomeCompleto &&
                    usr.UserName == registrarUsuarioCommand.Email &&
                    usr.Email == registrarUsuarioCommand.Email
                ),
                senhaPadrao
            ))
            .ReturnsAsync(IdentityResult.Success);

        UsuarioAutenticado usuarioAutenticado =
            new(usuarioEsperado.Id, usuarioEsperado.FullName, usuarioEsperado.Email);

        AccessToken accessTokenEsperado =
            new("token-simulacao", DateTime.UtcNow.AddMinutes(30), usuarioAutenticado);

        tokenProviderMock
            .Setup(t => t.GerarAccessToken(
                It.Is<Usuario>(usr =>
                    usr.FullName == registrarUsuarioCommand.NomeCompleto &&
                    usr.Email == registrarUsuarioCommand.Email
                )))
            .Returns(accessTokenEsperado);

        // Act
        Result<AccessToken> resultado = await handler.Handle(registrarUsuarioCommand, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(
            It.Is<Usuario>(usr =>
                usr.FullName == registrarUsuarioCommand.NomeCompleto && usr.UserName == registrarUsuarioCommand.Email &&
                usr.Email == registrarUsuarioCommand.Email), senhaPadrao), Times.Once);

        tokenProviderMock.Verify(t => t.GerarAccessToken(
            It.Is<Usuario>(usr =>
                usr.FullName == registrarUsuarioCommand.NomeCompleto && usr.Email == registrarUsuarioCommand.Email)), Times.Once);

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
        RegistrarUsuarioCommand registrarUsuarioCommand =
            new(fullNameCompletoPadrao, emailPadrao, senhaPadrao, senhaPadrao);

        IdentityError userDuplicateError = new()
        {
            Code = "DuplicateUserName",
            Description = "Já existe um usuário com esse nome."
        };

        userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao))
            .ReturnsAsync(IdentityResult.Failed(userDuplicateError));

        // Act
        Result<AccessToken> resultado = await handler.Handle(registrarUsuarioCommand, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao), Times.Once);
        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>()), Times.Never);

        string mensagemEsperada = MensagensErroAutenticacao.UsuarioJaExiste;
        List<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Requisição inválida", resultado.Errors[0].Message);
        Assert.AreEqual(1, mensagensDoResult.Count);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Registrar_Usuario_Com_Email_Duplicado()
    {
        // Arrange
        RegistrarUsuarioCommand registrarUsuarioCommand =
            new(fullNameCompletoPadrao, emailPadrao, senhaPadrao, senhaPadrao);

        IdentityError emailDuplicateError = new()
        {
            Code = "DuplicateEmail",
            Description = "Já existe um usuário com esse e-mail."
        };

        userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao))
            .ReturnsAsync(IdentityResult.Failed(emailDuplicateError));

        // Act
        Result<AccessToken> resultado = await handler.Handle(registrarUsuarioCommand, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao), Times.Once);
        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>()), Times.Never);

        string mensagemEsperada = MensagensErroAutenticacao.EmailJaExiste;
        List<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Requisição inválida", resultado.Errors[0].Message);
        Assert.AreEqual(1, mensagensDoResult.Count);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Registrar_Usuario_Com_Senha_Invalida()
    {
        // Arrange
        RegistrarUsuarioCommand registrarUsuarioCommand =
            new(fullNameCompletoPadrao, emailPadrao, senhaPadrao, senhaPadrao);

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
        Result<AccessToken> resultado = await handler.Handle(registrarUsuarioCommand, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao), Times.Once);
        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>()), Times.Never);

        string[] mensagensEsperadas =
        {
            MensagensErroAutenticacao.SenhaMuitoCurta,
            MensagensErroAutenticacao.SenhaRequerCaracterEspecial,
            MensagensErroAutenticacao.SenhaRequerNumero,
            MensagensErroAutenticacao.SenhaRequerMaiuscula,
            MensagensErroAutenticacao.SenhaRequerMinuscula
        };
        List<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual("Requisição inválida", resultado.Errors[0].Message);
        Assert.IsNotNull(mensagensDoResult);
        CollectionAssert.AreEquivalent(mensagensEsperadas, mensagensDoResult);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Registrar_Usuario_Com_Erro_Default()
    {
        // Arrange
        RegistrarUsuarioCommand registrarUsuarioCommand =
            new(fullNameCompletoPadrao, emailPadrao, senhaPadrao, senhaPadrao);

        IdentityError[] errosDesconhecidos =
        {
        new() { Code = "ErroInesperado", Description = "Erro inesperado no cadastro." }
        };

        userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao))
            .ReturnsAsync(IdentityResult.Failed(errosDesconhecidos));

        // Act
        Result<AccessToken> resultado = await handler.Handle(registrarUsuarioCommand, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao), Times.Once);
        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>()), Times.Never);

        string mensagemEsperada = "Erro inesperado no cadastro.";
        List<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual(1, mensagensDoResult.Count);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Registrar_Usuario_Com_Senhas_Diferentes()
    {
        // Arrange
        RegistrarUsuarioCommand registrarUsuarioCommand =
            new(fullNameCompletoPadrao, emailPadrao, senhaPadrao, "otraSenha123!");

        // Act
        Result<AccessToken> resultado = await handler.Handle(registrarUsuarioCommand, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao), Times.Never);
        tokenProviderMock.Verify(t => t.GerarAccessToken(It.IsAny<Usuario>()), Times.Never);

        string mensagemEsperada = "A confirmação de senha falhou.";
        List<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual(1, mensagensDoResult.Count);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }
    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Registrar_Usuario_E_Gerar_Não_Token()
    {
        // Arrange
        RegistrarUsuarioCommand registrarUsuarioCommand =
            new(fullNameCompletoPadrao, emailPadrao, senhaPadrao, senhaPadrao);

        Usuario usuarioEsperado = new()
        {
            FullName = registrarUsuarioCommand.NomeCompleto,
            UserName = registrarUsuarioCommand.Email,
            Email = registrarUsuarioCommand.Email
        };

        userManagerMock
            .Setup(u => u.CreateAsync(
                It.Is<Usuario>(usr =>
                    usr.FullName == registrarUsuarioCommand.NomeCompleto &&
                    usr.UserName == registrarUsuarioCommand.Email &&
                    usr.Email == registrarUsuarioCommand.Email
                ),
                senhaPadrao
            ))
            .ReturnsAsync(IdentityResult.Success);

        UsuarioAutenticado usuarioAutenticado =
            new(usuarioEsperado.Id, usuarioEsperado.FullName, usuarioEsperado.Email);

        AccessToken accessTokenEsperado =
            new("token-simulacao", DateTime.UtcNow.AddMinutes(30), usuarioAutenticado);

        tokenProviderMock
            .Setup(t => t.GerarAccessToken(
                It.Is<Usuario>(usr =>
                    usr.FullName == registrarUsuarioCommand.NomeCompleto &&
                    usr.Email == registrarUsuarioCommand.Email
                )));

        // Act
        Result<AccessToken> resultado = await handler.Handle(registrarUsuarioCommand, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(
            It.Is<Usuario>(usr =>
                usr.FullName == registrarUsuarioCommand.NomeCompleto && usr.UserName == registrarUsuarioCommand.Email &&
                usr.Email == registrarUsuarioCommand.Email), senhaPadrao), Times.Once);

        tokenProviderMock.Verify(t => t.GerarAccessToken(
            It.Is<Usuario>(usr =>
                usr.FullName == registrarUsuarioCommand.NomeCompleto && usr.Email == registrarUsuarioCommand.Email)), Times.Once);

        string mensagemEsperada = "Falha ao gerar token de acesso.";
        List<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.AreEqual(1, mensagensDoResult.Count);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }
    #endregion
}
