using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.AspNetCore.Http;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de Unidade de AutenticarUsuarioCommandHandler")]
public class AutenticarUsuarioCommandHandlerTestes
{
    private AutenticarUsuarioCommandHandler handler;

    private const string fullNamePadrao = "Tester Segundo";
    private const string emailPadrao = "emailTeste@teste.com";
    private const string senhaPadrao = "Teste123!";

    private Mock<SignInManager<Usuario>> signInManagerMock;
    private Mock<UserManager<Usuario>> userManagerMock;
    private Mock<ITokenProvider> tokenProviderMock;

    [TestInitialize]
    public void Setup()
    {
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

        tokenProviderMock = new Mock<ITokenProvider>();

        handler = new AutenticarUsuarioCommandHandler(
            signInManagerMock.Object,
                userManagerMock.Object,
                tokenProviderMock.Object
                );
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Sucesso_Quando_Autenticar_Usuario()
    {
        // Arrange
        AutenticarUsuarioCommand command = new(emailPadrao, senhaPadrao);

        Usuario usuarioEsperado = new()
        {
            FullName = fullNamePadrao,
            UserName = command.Email,
            Email = command.Email
        };

        userManagerMock
            .Setup(u => u.FindByEmailAsync(command.Email))
            .ReturnsAsync(usuarioEsperado);

        signInManagerMock
            .Setup(p => p.PasswordSignInAsync(usuarioEsperado.UserName, command.Senha, true, false))
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
                )))
            .Returns(accessTokenEsperado);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.FindByEmailAsync(command.Email), Times.Once);

        tokenProviderMock.Verify(t => t.GerarAccessToken(
            It.Is<Usuario>(usr =>
                usr.FullName == usuarioEsperado.FullName && usr.Email == usuarioEsperado.Email)), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(accessTokenEsperado.Chave, resultado.Value.Chave);
        Assert.AreEqual(accessTokenEsperado.Expiracao, resultado.Value.Expiracao);
        Assert.AreEqual(accessTokenEsperado.UsuarioAutenticado.Id, resultado.Value.UsuarioAutenticado.Id);
        Assert.AreEqual(accessTokenEsperado.UsuarioAutenticado.NomeCompleto, resultado.Value.UsuarioAutenticado.NomeCompleto);
        Assert.AreEqual(accessTokenEsperado.UsuarioAutenticado.Email, resultado.Value.UsuarioAutenticado.Email);
    }
}
