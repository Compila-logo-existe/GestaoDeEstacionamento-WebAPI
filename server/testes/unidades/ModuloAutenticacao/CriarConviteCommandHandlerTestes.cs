using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Testes.Unidades.Compartilhado;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de Unidade de CriarConviteCommandHandler")]
public class CriarConviteCommandHandlerTestes
{
    private CriarConviteCommandHandler handler = null!;

    private const string emailPadrao = "emailTeste@teste.com";
    private const string cargoPadrao = "Admin";
    private const string slug = "eTeste";
    private Tenant tenantPadrao = null!;

    private Mock<IRepositorioConvite> repositorioConvite = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<IUnitOfWork> unitOfWorkMock = null!;
    private Mock<ILogger<CriarConviteCommand>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        tenantPadrao = new(Guid.NewGuid(), "Empresa Teste", "00.000.000/0000-00",
            slug, null, DateTime.UtcNow);

        repositorioConvite = new Mock<IRepositorioConvite>();
        tenantProviderMock = new Mock<ITenantProvider>();
        unitOfWorkMock = new Mock<IUnitOfWork>();
        loggerMock = new Mock<ILogger<CriarConviteCommand>>();

        handler = new CriarConviteCommandHandler(
            repositorioConvite.Object,
            tenantProviderMock.Object,
            unitOfWorkMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Sucesso_Quando_Criar_Convite()
    {
        // Arrange
        CriarConviteCommand command = new(tenantPadrao.Id, emailPadrao, cargoPadrao);
        Guid usuarioAutenticadoId = Guid.NewGuid();

        tenantProviderMock
            .SetupGet(p => p.UsuarioId)
            .Returns(usuarioAutenticadoId);

        string tokenConvite = Convert.ToHexString(Guid.NewGuid().ToByteArray());
        DateTime expira = DateTime.UtcNow.AddDays(7);

        ConviteRegistro convite = new()
        {
            UsuarioEmissorId = usuarioAutenticadoId,
            EmailConvidado = command.EmailConvidado,
            NomeCargo = command.NomeCargo,
            TokenConvite = tokenConvite,
            DataExpiracaoUtc = expira
        };

        convite.VincularTenant(tenantPadrao.Id);

        repositorioConvite
            .Setup(Setup => Setup.CriarAsync(It.Is<ConviteRegistro>(c =>
            c.UsuarioEmissorId == convite.UsuarioEmissorId && c.EmailConvidado == convite.EmailConvidado &&
            c.NomeCargo == convite.NomeCargo && c.TokenConvite == convite.TokenConvite &&
            c.DataExpiracaoUtc == convite.DataExpiracaoUtc), It.IsAny<CancellationToken>()
            ));

        unitOfWorkMock
            .Setup(p => p.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        Result<(string, DateTime)> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        repositorioConvite.Verify(p => p.CriarAsync(It.IsAny<ConviteRegistro>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(p => p.CommitAsync(), Times.Once);

        Assert.IsTrue(resultado.IsSuccess);
    }

    [TestMethod]
    public async Task Handle_Deve_Falhar_Quando_Tenant_Nao_Informado()
    {
        // Arrange
        CriarConviteCommand command = new(Guid.Empty, emailPadrao, cargoPadrao);

        // Act
        Result<(string, DateTime)> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        repositorioConvite.Verify(p => p.CriarAsync(It.IsAny<ConviteRegistro>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(p => p.CommitAsync(), Times.Never);

        const string mensagemEsperada = MensagensErro.TenantNaoInformado;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsTrue(resultado.IsFailed);
        Assert.IsTrue(mensagensDoResult.Count >= 1);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Falhar_Quando_Usuario_Nao_Identificado()
    {
        // Arrange
        CriarConviteCommand command = new(tenantPadrao.Id, emailPadrao, cargoPadrao);
        Guid usuarioAutenticadoId = Guid.NewGuid();

        tenantProviderMock
            .SetupGet(p => p.UsuarioId)
            .Returns((Guid?)null);

        // Act
        Result<(string, DateTime)> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        repositorioConvite.Verify(p => p.CriarAsync(It.IsAny<ConviteRegistro>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(p => p.CommitAsync(), Times.Never);

        const string mensagemEsperada = MensagensErro.UsuarioNaoIdentificado;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsTrue(resultado.IsFailed);
        Assert.IsTrue(mensagensDoResult.Count >= 1);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }

    [TestMethod]
    public async Task Handle_Deve_Retornar_Falha_Quando_Ocorrer_Excecao_Durante_Criacao_Convite()
    {
        // Arrange
        CriarConviteCommand command = new(tenantPadrao.Id, emailPadrao, cargoPadrao);
        Guid usuarioAutenticadoId = Guid.NewGuid();

        tenantProviderMock
            .SetupGet(p => p.UsuarioId)
            .Returns(usuarioAutenticadoId);

        string tokenConvite = Convert.ToHexString(Guid.NewGuid().ToByteArray());
        DateTime expira = DateTime.UtcNow.AddDays(7);

        ConviteRegistro convite = new()
        {
            UsuarioEmissorId = usuarioAutenticadoId,
            EmailConvidado = command.EmailConvidado,
            NomeCargo = command.NomeCargo,
            TokenConvite = tokenConvite,
            DataExpiracaoUtc = expira
        };

        convite.VincularTenant(tenantPadrao.Id);

        repositorioConvite
            .Setup(r => r.CriarAsync(It.IsAny<ConviteRegistro>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Ocorreu um erro."));

        unitOfWorkMock
            .Setup(p => p.CommitAsync())
            .Returns(Task.CompletedTask);

        // Act
        Result<(string, DateTime)> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        repositorioConvite.Verify(p => p.CriarAsync(It.IsAny<ConviteRegistro>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(p => p.CommitAsync(), Times.Never);
        unitOfWorkMock.Verify(p => p.RollbackAsync(), Times.Once);

        const string mensagemEsperada = MensagensErro.MensagemExcecao;
        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsTrue(resultado.IsFailed);
        Assert.IsTrue(mensagensDoResult.Count >= 1);
        Assert.AreEqual(mensagemEsperada, mensagensDoResult[0]);
    }
}
