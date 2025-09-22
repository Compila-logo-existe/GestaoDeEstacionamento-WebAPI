using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de Unidade de AceitarConviteCommandHandler")]
public class AceitarConviteCommandHandlerTestes
{
    private AceitarConviteCommandHandler handler = null!;

    private const string fullNamePadrao = "Tester Segundo";
    private const string emailPadrao = "emailTeste@teste.com";
    private const string senhaPadrao = "Teste123!";
    private const string cargoPadrao = "Admin";
    private const string slug = "eTeste";
    private Tenant tenantPadrao = null!;

    private Mock<IRepositorioConvite> repositorioConvite = null!;
    private Mock<IRepositorioTenant> repositorioTenantMock = null!;
    private Mock<IRepositorioUsuarioTenant> repositorioUsuarioTenantMock = null!;
    private Mock<UserManager<Usuario>> userManagerMock = null!;
    private Mock<ITokenProvider> tokenProviderMock = null!;
    private Mock<IUnitOfWork> unitOfWorkMock = null!;
    private Mock<ILogger<AceitarConviteCommandHandler>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        tenantPadrao = new(
            Guid.NewGuid(),
            "Empresa Teste",
            "00.000.000/0000-00",
            slug,
            null,
            DateTime.UtcNow);

        repositorioConvite = new Mock<IRepositorioConvite>();
        repositorioUsuarioTenantMock = new Mock<IRepositorioUsuarioTenant>();
        repositorioTenantMock = new Mock<IRepositorioTenant>();
        tokenProviderMock = new Mock<ITokenProvider>();
        unitOfWorkMock = new Mock<IUnitOfWork>();
        loggerMock = new Mock<ILogger<AceitarConviteCommandHandler>>();

        userManagerMock = new Mock<UserManager<Usuario>>(
            new Mock<IUserStore<Usuario>>().Object,
            null!, null!, null!, null!, null!, null!, null!, null!
        );

        handler = new AceitarConviteCommandHandler(
            repositorioConvite.Object,
            repositorioTenantMock.Object,
            repositorioUsuarioTenantMock.Object,
            userManagerMock.Object,
            tokenProviderMock.Object,
            unitOfWorkMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Sucesso_Quando_Aceitar_Convite_Usuario_Novo()
    {
        // Arrange
        string tokenConvite = Convert.ToHexString(Guid.NewGuid().ToByteArray());
        DateTime dataExpiracaoUtc = DateTime.UtcNow.AddDays(7);

        AceitarConviteCommand command = new(tokenConvite, fullNamePadrao, senhaPadrao, senhaPadrao);

        ConviteRegistro convite = new()
        {
            UsuarioEmissorId = Guid.NewGuid(),
            EmailConvidado = emailPadrao,
            NomeCargo = cargoPadrao,
            TokenConvite = tokenConvite,
            DataExpiracaoUtc = dataExpiracaoUtc
        };
        convite.VincularTenant(tenantPadrao.Id);

        repositorioConvite
            .Setup(p => p.ObterAtivoPorTokenAsync(tokenConvite, It.IsAny<CancellationToken>()))
            .ReturnsAsync(convite);

        userManagerMock
            .Setup(u => u.FindByEmailAsync(It.Is<string>(e => e == emailPadrao)))
            .ReturnsAsync((Usuario?)null);

        userManagerMock
            .Setup(u => u.CreateAsync(
                It.Is<Usuario>(usr =>
                    usr.Email == emailPadrao &&
                    usr.UserName == emailPadrao &&
                    usr.EmailConfirmed == true &&
                    !string.IsNullOrWhiteSpace(usr.FullName) &&
                    usr.FullName == fullNamePadrao.Trim()),
                It.Is<string>(s => s == senhaPadrao)))
            .ReturnsAsync(IdentityResult.Success);

        userManagerMock
            .Setup(u => u.GetRolesAsync(It.Is<Usuario>(usr => usr.Email == emailPadrao)))
            .ReturnsAsync(new List<string>());

        userManagerMock
            .Setup(u => u.AddToRoleAsync(
                It.Is<Usuario>(usr => usr.Email == emailPadrao),
                It.Is<string>(r => r == cargoPadrao)))
            .ReturnsAsync(IdentityResult.Success);

        repositorioTenantMock
            .Setup(r => r.ObterPorIdAsync(It.Is<Guid>(g => g == tenantPadrao.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenantPadrao);

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(It.IsAny<Guid>(), It.Is<Guid>(g => g == tenantPadrao.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        VinculoUsuarioTenant? vinculoCapturado = null;
        repositorioUsuarioTenantMock
            .Setup(r => r.CadastrarRegistroAsync(It.IsAny<VinculoUsuarioTenant>()))
            .Callback<VinculoUsuarioTenant>(v => vinculoCapturado = v)
            .Returns(Task.CompletedTask);

        repositorioConvite
            .Setup(r => r.MarcarComoUtilizadoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert 
        repositorioConvite.Verify(p => p.ObterAtivoPorTokenAsync(tokenConvite, It.IsAny<CancellationToken>()), Times.Once);
        userManagerMock.Verify(u => u.FindByEmailAsync(emailPadrao), Times.Once);
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao), Times.Once);
        userManagerMock.Verify(u => u.GetRolesAsync(It.IsAny<Usuario>()), Times.Once);
        userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<Usuario>(), cargoPadrao), Times.Once);
        repositorioTenantMock.Verify(r => r.ObterPorIdAsync(tenantPadrao.Id, It.IsAny<CancellationToken>()), Times.Once);
        repositorioUsuarioTenantMock.Verify(r => r.UsuarioPertenceAoTenantAsync(It.IsAny<Guid>(), tenantPadrao.Id, It.IsAny<CancellationToken>()), Times.Once);
        repositorioUsuarioTenantMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<VinculoUsuarioTenant>()), Times.Once);
        repositorioConvite.Verify(r => r.MarcarComoUtilizadoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        tokenProviderMock.Verify(p => p.GerarAccessToken(It.IsAny<Usuario>(), tenantPadrao.Id), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.IsNotNull(vinculoCapturado);
        Assert.AreEqual(cargoPadrao, vinculoCapturado!.NomeCargo);
        Assert.AreEqual(slug, vinculoCapturado.Slug);
        Assert.AreEqual(tenantPadrao.Id, vinculoCapturado.TenantId);
    }

    [TestMethod]
    public async Task Handle_Deve_Falhar_Quando_Convite_Invalido_ou_Expirado()
    {
        // Arrange
        string tokenConvite = "TOKEN_INVALIDO";
        AceitarConviteCommand command = new(tokenConvite, fullNamePadrao, senhaPadrao, senhaPadrao);

        repositorioConvite
            .Setup(p => p.ObterAtivoPorTokenAsync(tokenConvite, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConviteRegistro?)null);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.FindByEmailAsync(It.IsAny<string>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        tokenProviderMock.Verify(p => p.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);
        repositorioConvite.Verify(r => r.MarcarComoUtilizadoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_Deve_Falhar_Quando_Confirmar_Senha_Diferente()
    {
        // Arrange
        string tokenConvite = Convert.ToHexString(Guid.NewGuid().ToByteArray());
        AceitarConviteCommand command = new(tokenConvite, fullNamePadrao, senhaPadrao, "otraSenha!");

        ConviteRegistro convite = new()
        {
            UsuarioEmissorId = Guid.NewGuid(),
            EmailConvidado = emailPadrao,
            NomeCargo = cargoPadrao,
            TokenConvite = tokenConvite,
            DataExpiracaoUtc = DateTime.UtcNow.AddDays(5)
        };
        convite.VincularTenant(tenantPadrao.Id);

        repositorioConvite
            .Setup(p => p.ObterAtivoPorTokenAsync(tokenConvite, It.IsAny<CancellationToken>()))
            .ReturnsAsync(convite);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        tokenProviderMock.Verify(p => p.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);
        repositorioConvite.Verify(r => r.MarcarComoUtilizadoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_Deve_Falhar_Quando_Tenant_Nao_Encontrado()
    {
        // Arrange
        string tokenConvite = Convert.ToHexString(Guid.NewGuid().ToByteArray());
        AceitarConviteCommand command = new(tokenConvite, fullNamePadrao, senhaPadrao, senhaPadrao);

        ConviteRegistro convite = new()
        {
            UsuarioEmissorId = Guid.NewGuid(),
            EmailConvidado = emailPadrao,
            NomeCargo = cargoPadrao,
            TokenConvite = tokenConvite,
            DataExpiracaoUtc = DateTime.UtcNow.AddDays(1)
        };
        convite.VincularTenant(tenantPadrao.Id);

        repositorioConvite
            .Setup(p => p.ObterAtivoPorTokenAsync(tokenConvite, It.IsAny<CancellationToken>()))
            .ReturnsAsync(convite);

        userManagerMock
            .Setup(u => u.FindByEmailAsync(emailPadrao))
            .ReturnsAsync((Usuario?)null);

        userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao))
            .ReturnsAsync(IdentityResult.Success);

        repositorioTenantMock
            .Setup(r => r.ObterPorIdAsync(tenantPadrao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        repositorioUsuarioTenantMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<VinculoUsuarioTenant>()), Times.Never);
        repositorioConvite.Verify(r => r.MarcarComoUtilizadoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        tokenProviderMock.Verify(p => p.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_Deve_Falhar_Quando_CreateAsync_Retornar_Falha()
    {
        // Arrange
        string tokenConvite = Convert.ToHexString(Guid.NewGuid().ToByteArray());
        AceitarConviteCommand command = new(tokenConvite, fullNamePadrao, senhaPadrao, senhaPadrao);

        ConviteRegistro convite = new()
        {
            UsuarioEmissorId = Guid.NewGuid(),
            EmailConvidado = emailPadrao,
            NomeCargo = cargoPadrao,
            TokenConvite = tokenConvite,
            DataExpiracaoUtc = DateTime.UtcNow.AddDays(2)
        };
        convite.VincularTenant(tenantPadrao.Id);

        repositorioConvite
            .Setup(p => p.ObterAtivoPorTokenAsync(tokenConvite, It.IsAny<CancellationToken>()))
            .ReturnsAsync(convite);

        userManagerMock
            .Setup(u => u.FindByEmailAsync(emailPadrao))
            .ReturnsAsync((Usuario?)null);

        userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "Email já cadastrado" }));

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        tokenProviderMock.Verify(p => p.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);

        Assert.IsTrue(resultado.IsFailed);
        Assert.IsTrue(resultado.Errors.Any(e => e.Message.Contains("DuplicateEmail")));
    }

    [TestMethod]
    public async Task Handle_Deve_Falhar_Quando_Senha_Incorreta_Para_Usuario_Existente()
    {
        // Arrange
        string tokenConvite = Convert.ToHexString(Guid.NewGuid().ToByteArray());
        AceitarConviteCommand command = new(tokenConvite, fullNamePadrao, senhaPadrao, senhaPadrao);

        ConviteRegistro convite = new()
        {
            UsuarioEmissorId = Guid.NewGuid(),
            EmailConvidado = emailPadrao,
            NomeCargo = cargoPadrao,
            TokenConvite = tokenConvite,
            DataExpiracaoUtc = DateTime.UtcNow.AddDays(4)
        };
        convite.VincularTenant(tenantPadrao.Id);

        Usuario usuarioExistente = new()
        {
            Id = Guid.NewGuid(),
            Email = emailPadrao,
            UserName = emailPadrao,
            EmailConfirmed = true,
            FullName = "Nome Já Preenchido",
            AccessTokenVersionId = Guid.NewGuid()
        };

        repositorioConvite
            .Setup(p => p.ObterAtivoPorTokenAsync(tokenConvite, It.IsAny<CancellationToken>()))
            .ReturnsAsync(convite);

        userManagerMock
            .Setup(u => u.FindByEmailAsync(emailPadrao))
            .ReturnsAsync(usuarioExistente);

        userManagerMock
            .Setup(u => u.CheckPasswordAsync(usuarioExistente, senhaPadrao))
            .ReturnsAsync(false);

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Usuario>(), It.IsAny<string>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        tokenProviderMock.Verify(p => p.GerarAccessToken(It.IsAny<Usuario>(), It.IsAny<Guid>()), Times.Never);

        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Handle_Deve_Rollback_Logar_E_Retornar_Falha_Quando_Ocorrencia_De_Excecao_Geral_Apos_Commit()
    {
        // Arrange
        string tokenConvite = Convert.ToHexString(Guid.NewGuid().ToByteArray());
        AceitarConviteCommand command = new(tokenConvite, fullNamePadrao, senhaPadrao, senhaPadrao);

        ConviteRegistro convite = new()
        {
            UsuarioEmissorId = Guid.NewGuid(),
            EmailConvidado = emailPadrao,
            NomeCargo = cargoPadrao,
            TokenConvite = tokenConvite,
            DataExpiracaoUtc = DateTime.UtcNow.AddDays(2)
        };
        convite.VincularTenant(tenantPadrao.Id);

        repositorioConvite
            .Setup(p => p.ObterAtivoPorTokenAsync(tokenConvite, It.IsAny<CancellationToken>()))
            .ReturnsAsync(convite);

        userManagerMock
            .Setup(u => u.FindByEmailAsync(emailPadrao))
            .ReturnsAsync((Usuario?)null);

        userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<Usuario>(), senhaPadrao))
            .ReturnsAsync(IdentityResult.Success);

        userManagerMock
            .Setup(u => u.GetRolesAsync(It.IsAny<Usuario>()))
            .ReturnsAsync(new List<string>());

        userManagerMock
            .Setup(u => u.AddToRoleAsync(It.IsAny<Usuario>(), cargoPadrao))
            .ReturnsAsync(IdentityResult.Success);

        repositorioTenantMock
            .Setup(r => r.ObterPorIdAsync(tenantPadrao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenantPadrao);

        repositorioUsuarioTenantMock
            .Setup(r => r.UsuarioPertenceAoTenantAsync(It.IsAny<Guid>(), tenantPadrao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        repositorioUsuarioTenantMock
            .Setup(r => r.CadastrarRegistroAsync(It.IsAny<VinculoUsuarioTenant>()))
            .Returns(Task.CompletedTask);

        repositorioConvite
            .Setup(r => r.MarcarComoUtilizadoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(u => u.RollbackAsync())
            .Returns(Task.CompletedTask);

        tokenProviderMock
            .Setup(p => p.GerarAccessToken(It.IsAny<Usuario>(), tenantPadrao.Id))
            .ThrowsAsync(new Exception("boom"));

        // Act
        Result<AccessToken> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);

        Assert.IsTrue(resultado.IsFailed);
    }
}
