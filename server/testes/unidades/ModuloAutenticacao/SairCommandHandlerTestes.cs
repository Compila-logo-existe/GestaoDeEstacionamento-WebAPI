using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using Microsoft.AspNetCore.Http;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de Unidade de SairCommandHandler")]
public class SairCommandHandlerTestes
{
    private SairCommandHandler handler = null!;

    private Mock<UserManager<Usuario>> userManagerMock = null!;
    private Mock<SignInManager<Usuario>> signInManagerMock = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<IRefreshTokenProvider> refreshTokenProviderMock = null!;
    private Mock<IUnitOfWork> unitOfWorkMock = null!;
    private Mock<ILogger<SairCommand>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        userManagerMock = new Mock<UserManager<Usuario>>(
            new Mock<IUserStore<Usuario>>().Object,
            null!, null!, null!, null!, null!, null!, null!, null!
        );

        signInManagerMock = new Mock<SignInManager<Usuario>>(
            userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<Usuario>>(),
            null!, null!, null!, null!
        );

        tenantProviderMock = new Mock<ITenantProvider>();
        refreshTokenProviderMock = new Mock<IRefreshTokenProvider>();
        unitOfWorkMock = new Mock<IUnitOfWork>();
        loggerMock = new Mock<ILogger<SairCommand>>();

        handler = new SairCommandHandler(
            userManagerMock.Object,
            signInManagerMock.Object,
            tenantProviderMock.Object,
            refreshTokenProviderMock.Object,
            unitOfWorkMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Sucesso_Quando_Usuario_Sair()
    {
        // Arrange
        SairCommand comando = new();
        Guid usuarioAutenticadoId = Guid.NewGuid();

        tenantProviderMock
            .SetupGet(p => p.UsuarioId)
            .Returns(usuarioAutenticadoId);

        signInManagerMock
            .Setup(p => p.SignOutAsync());

        refreshTokenProviderMock
            .Setup(p => p.RevogarTokensUsuarioAsync(usuarioAutenticadoId, CancellationToken.None));

        Usuario usuario = new() { Id = usuarioAutenticadoId };
        userManagerMock
            .Setup(p => p.FindByIdAsync(usuarioAutenticadoId.ToString()))
            .ReturnsAsync(usuario);

        userManagerMock
            .Setup(p => p.UpdateAsync(It.IsAny<Usuario>()))
            .ReturnsAsync(IdentityResult.Success);

        unitOfWorkMock
            .Setup(p => p.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        Result resultado = await handler.Handle(comando, CancellationToken.None);

        // Assert
        signInManagerMock.Verify(p => p.SignOutAsync(), Times.Once);
        refreshTokenProviderMock.Verify(p => p.RevogarTokensUsuarioAsync(usuarioAutenticadoId, CancellationToken.None), Times.Once);
        userManagerMock.Verify(p => p.FindByIdAsync(usuarioAutenticadoId.ToString()), Times.Once);
        userManagerMock.Verify(p => p.UpdateAsync(It.Is<Usuario>(u => u == usuario)), Times.Once);
        unitOfWorkMock.Verify(p => p.CommitAsync(), Times.Once);

        Assert.IsTrue(resultado.IsSuccess);
    }

    [TestMethod]
    public async Task Handle_Deve_Falhar_Quando_Usuario_Nao_Identificado()
    {
        // Arrange
        SairCommand comando = new();

        tenantProviderMock
            .SetupGet(p => p.UsuarioId)
            .Returns((Guid?)null);

        // Act
        Result resultado = await handler.Handle(comando, CancellationToken.None);

        // Assert
        signInManagerMock.Verify(p => p.SignOutAsync(), Times.Never);
        refreshTokenProviderMock.Verify(p => p.RevogarTokensUsuarioAsync(It.IsAny<Guid>(), CancellationToken.None), Times.Never);
        userManagerMock.Verify(p => p.FindByIdAsync(It.IsAny<string>()), Times.Never);
        userManagerMock.Verify(p => p.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        unitOfWorkMock.Verify(p => p.CommitAsync(), Times.Never);

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_Deve_Falhar_Quando_Usuario_Nao_Encontrado()
    {
        // Arrange
        SairCommand comando = new();
        Guid usuarioAutenticadoId = Guid.NewGuid();

        tenantProviderMock.SetupGet(p => p.UsuarioId).Returns(usuarioAutenticadoId);

        signInManagerMock.Setup(p => p.SignOutAsync()).Returns(Task.CompletedTask);
        refreshTokenProviderMock.Setup(p => p.RevogarTokensUsuarioAsync(usuarioAutenticadoId, CancellationToken.None));

        userManagerMock
            .Setup(p => p.FindByIdAsync(usuarioAutenticadoId.ToString()))
            .ReturnsAsync((Usuario?)null);

        // Act
        Result resultado = await handler.Handle(comando, CancellationToken.None);

        // Assert
        userManagerMock.Verify(p => p.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        unitOfWorkMock.Verify(p => p.CommitAsync(), Times.Never);

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_Deve_Falhar_Quando_Update_De_Token_De_Acesso_Falhar()
    {
        // Arrange
        SairCommand comando = new();
        Guid usuarioAutenticadoId = Guid.NewGuid();

        tenantProviderMock.SetupGet(p => p.UsuarioId).Returns(usuarioAutenticadoId);

        signInManagerMock.Setup(p => p.SignOutAsync()).Returns(Task.CompletedTask);
        refreshTokenProviderMock.Setup(p => p.RevogarTokensUsuarioAsync(usuarioAutenticadoId, It.IsAny<CancellationToken>()));

        Usuario usuario = new() { Id = usuarioAutenticadoId };
        userManagerMock.Setup(p => p.FindByIdAsync(usuarioAutenticadoId.ToString())).ReturnsAsync(usuario);
        userManagerMock.Setup(p => p.UpdateAsync(usuario)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "erro" }));

        // Act
        Result resultado = await handler.Handle(comando, CancellationToken.None);

        // Assert
        unitOfWorkMock.Verify(p => p.CommitAsync(), Times.Never);

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Ocorrer_Excecao_Durante_Saida()
    {
        // Arrange
        SairCommand command = new();
        Guid usuarioAutenticadoId = Guid.NewGuid();

        tenantProviderMock.SetupGet(p => p.UsuarioId).Returns(usuarioAutenticadoId);

        signInManagerMock
            .Setup(s => s.SignOutAsync())
            .ThrowsAsync(new Exception("Ocorreu um erro."));

        // Act
        Result resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        refreshTokenProviderMock.Verify(r => r.RevogarTokensUsuarioAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        userManagerMock.Verify(u => u.FindByIdAsync(It.IsAny<string>()), Times.Never);
        userManagerMock.Verify(u => u.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);

        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.IsTrue(mensagensDoResult.Count >= 1);
    }
}
