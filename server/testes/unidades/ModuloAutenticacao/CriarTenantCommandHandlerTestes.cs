using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de Unidade de CriarTenantCommandHandler")]
public class CriarTenantCommandHandlerTestes
{
    private CriarTenantCommandHandler handler = null!;

    private const string nomeTenant = "Empresa Teste Ltda";
    private const string cnpjTenant = "12.345.678/0001-90";
    private const string slugInformado = "MinhaEmpresa";
    private const string dominioPersonalizadoInformado = "MinhaEmpresa.COM.Br";
    private readonly Guid usuarioIdPadrao = Guid.NewGuid();

    private Mock<IRepositorioTenant> repositorioTenantMock = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<IUnitOfWork> unitOfWorkMock = null!;
    private Mock<ILogger<CriarTenantCommand>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        repositorioTenantMock = new Mock<IRepositorioTenant>();
        tenantProviderMock = new Mock<ITenantProvider>();
        unitOfWorkMock = new Mock<IUnitOfWork>();
        loggerMock = new Mock<ILogger<CriarTenantCommand>>();

        tenantProviderMock
            .SetupGet(p => p.UsuarioId)
            .Returns(usuarioIdPadrao);

        handler = new CriarTenantCommandHandler(
            repositorioTenantMock.Object,
            tenantProviderMock.Object,
            unitOfWorkMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Sucesso_Quando_Criar_Tenant()
    {
        // Arrange

        CriarTenantCommand command = new(
            nomeTenant,
            cnpjTenant,
            slugInformado,
            dominioPersonalizadoInformado
        );

        Tenant? tenantCapturado = null;

        repositorioTenantMock
            .Setup(r => r.CriarAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .Callback<Tenant, CancellationToken>((t, _) => tenantCapturado = t)
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        Result<Guid> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert 
        repositorioTenantMock.Verify(r => r.CriarAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        Assert.IsTrue(resultado.IsSuccess);
        Assert.IsNotNull(tenantCapturado);
        Assert.AreEqual(slugInformado.ToLowerInvariant(), tenantCapturado!.SlugSubdominio);
        Assert.AreEqual(dominioPersonalizadoInformado.ToLowerInvariant(), tenantCapturado.DominioPersonalizado);
        Assert.AreNotEqual(Guid.Empty, tenantCapturado.Id);
        Assert.AreEqual(tenantCapturado.Id, resultado.Value);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Sucesso_Quando_Criar_Tenant_Sem_Dominio_Personalizado()
    {
        // Arrange
        CriarTenantCommand command = new(
            nomeTenant,
            cnpjTenant,
            slugInformado,
            null
        );

        Tenant? tenantCapturado = null;

        repositorioTenantMock
            .Setup(r => r.CriarAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .Callback<Tenant, CancellationToken>((t, _) => tenantCapturado = t)
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        Result<Guid> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        repositorioTenantMock.Verify(r => r.CriarAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        Assert.IsTrue(resultado.IsSuccess);
        Assert.IsNotNull(tenantCapturado);
        Assert.AreEqual(slugInformado.ToLowerInvariant(), tenantCapturado!.SlugSubdominio);
        Assert.IsNull(tenantCapturado.DominioPersonalizado);
        Assert.AreEqual(tenantCapturado.Id, resultado.Value);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Usuario_Nao_Identificado_No_Tenant()
    {
        // Arrange
        CriarTenantCommand command = new(
            nomeTenant,
            cnpjTenant,
            slugInformado,
            null
        );

        tenantProviderMock
            .SetupGet(p => p.UsuarioId);

        // Act
        Result<Guid> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert 
        repositorioTenantMock.Verify(r => r.CriarAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);

        const string mensagemEsperada = "Usuário não identificado no tenant.";
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
    public async Task Handle_Deve_Retornar_Falha_Quando_Ocorrer_Excecao_Durante_Criacao()
    {
        // Arrange
        CriarTenantCommand command = new(
            nomeTenant,
            cnpjTenant,
            slugInformado,
            dominioPersonalizadoInformado
        );

        repositorioTenantMock
            .Setup(r => r.CriarAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("falha no repositório"));

        unitOfWorkMock
            .Setup(u => u.RollbackAsync())
            .Returns(Task.CompletedTask);

        // Act
        Result<Guid> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);

        Assert.IsTrue(resultado.IsFailed);
    }
}
