using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.AspNetCore.Http;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de Unidade de SairCommandHandler")]
public class SairCommandHandlerTestes
{
    private SairCommandHandler handler;

    private Mock<SignInManager<Usuario>> signInManagerMock;
    private Mock<UserManager<Usuario>> userManagerMock;

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

        handler = new SairCommandHandler(signInManagerMock.Object);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Sucesso_Quando_Usuario_Sair()
    {
        // Arrange
        SairCommand sairCommand = new();

        signInManagerMock
            .Setup(p => p.SignOutAsync());

        // Act
        Result<AccessToken> resultado = await handler.Handle(sairCommand, CancellationToken.None);

        // Assert
        signInManagerMock.Verify(p => p.SignOutAsync(), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
    }
}
