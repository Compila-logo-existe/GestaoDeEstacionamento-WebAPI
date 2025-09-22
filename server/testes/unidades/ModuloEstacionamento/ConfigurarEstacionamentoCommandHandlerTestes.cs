using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Handlers;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Testes.Unidades.Compartilhado;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloEstacionamento;

[TestClass]
[TestCategory("Testes de Unidade de ConfigurarEstacionamentoCommandHandler")]
public class ConfigurarEstacionamentoCommandHandlerTestes
{
    private ConfigurarEstacionamentoCommandHandler handler = null!;

    private const string nomeEstacionamentoPadrao = "Estacionamento Central";
    private const int quantidadeVagasPadrao = 6;
    private const int zonasTotaisPadrao = 3;
    private const int vagasPorZonaPadrao = 2;

    private readonly Guid tenantIdPadrao = Guid.NewGuid();
    private readonly Guid usuarioIdPadrao = Guid.NewGuid();

    private Mock<IValidator<ConfigurarEstacionamentoCommand>> validatorMock = null!;
    private Mock<IMapper> mapperMock = null!;
    private Mock<IRepositorioEstacionamento> repositorioEstacionamentoMock = null!;
    private Mock<IRepositorioVaga> repositorioVagaMock = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<IDistributedCache> cacheMock = null!;
    private Mock<IUnitOfWork> unitOfWorkMock = null!;
    private Mock<ILogger<ConfigurarEstacionamentoCommandHandler>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        validatorMock = new Mock<IValidator<ConfigurarEstacionamentoCommand>>();
        mapperMock = new Mock<IMapper>();
        repositorioEstacionamentoMock = new Mock<IRepositorioEstacionamento>();
        repositorioVagaMock = new Mock<IRepositorioVaga>();
        tenantProviderMock = new Mock<ITenantProvider>();
        cacheMock = new Mock<IDistributedCache>();
        unitOfWorkMock = new Mock<IUnitOfWork>();
        loggerMock = new Mock<ILogger<ConfigurarEstacionamentoCommandHandler>>();

        tenantProviderMock.SetupGet(p => p.TenantId)
            .Returns(tenantIdPadrao);
        tenantProviderMock.SetupGet(p => p.UsuarioId)
            .Returns(usuarioIdPadrao);

        handler = new ConfigurarEstacionamentoCommandHandler(
            validatorMock.Object,
            mapperMock.Object,
            repositorioEstacionamentoMock.Object,
            repositorioVagaMock.Object,
            tenantProviderMock.Object,
            cacheMock.Object,
            unitOfWorkMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Configurar_Deve_Retornar_Sucesso()
    {
        // Arrange
        ConfigurarEstacionamentoCommand command = new(nomeEstacionamentoPadrao, quantidadeVagasPadrao,
            zonasTotaisPadrao, vagasPorZonaPadrao);

        validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Guid estacionamentoIdGerado = Guid.NewGuid();
        Estacionamento estacionamentoMapeado = new() { Id = estacionamentoIdGerado, Nome = nomeEstacionamentoPadrao };

        mapperMock
            .Setup(m => m.Map<Estacionamento>(command))
            .Returns(estacionamentoMapeado);

        repositorioEstacionamentoMock
            .Setup(r => r.CadastrarRegistroAsync(estacionamentoMapeado));

        repositorioVagaMock
            .Setup(r => r.CadastrarRegistroAsync(It.IsAny<Vaga>()));

        unitOfWorkMock
            .Setup(u => u.CommitAsync());

        cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));

        ConfigurarEstacionamentoResult resultadoMapeado =
            new(estacionamentoIdGerado, nomeEstacionamentoPadrao, quantidadeVagasPadrao,
            ImmutableList<ZonaEstacionamentoDto>.Empty);

        mapperMock
            .Setup(m => m.Map<ConfigurarEstacionamentoResult>(estacionamentoMapeado))
            .Returns(resultadoMapeado);

        // Act
        Result<ConfigurarEstacionamentoResult> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        repositorioEstacionamentoMock.Verify(r => r.CadastrarRegistroAsync(estacionamentoMapeado), Times.Once);
        repositorioVagaMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<Vaga>()), Times.Exactly(quantidadeVagasPadrao));
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(estacionamentoIdGerado, resultado.Value.Id);
        Assert.AreEqual(nomeEstacionamentoPadrao, resultado.Value.Nome);
        Assert.AreEqual(quantidadeVagasPadrao, resultado.Value.QuantidadeDeVagasCriadas);
    }

    [TestMethod]
    public async Task Configurar_Deve_Retornar_Falha_Quando_Tenant_Nao_Informado()
    {
        // Arrange
        tenantProviderMock.SetupGet(p => p.TenantId);

        ConfigurarEstacionamentoCommand command = new(nomeEstacionamentoPadrao, 1, 1, 1);

        // Act
        Result<ConfigurarEstacionamentoResult> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        repositorioEstacionamentoMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<Estacionamento>()), Times.Never);
        repositorioVagaMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<Vaga>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);

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
    public async Task Configurar_Deve_Retornar_Falha_Quando_Usuario_Nao_Identificado()
    {
        // Arrange
        tenantProviderMock.SetupGet(p => p.UsuarioId);

        ConfigurarEstacionamentoCommand command = new(nomeEstacionamentoPadrao, 1, 1, 1);

        // Act
        Result<ConfigurarEstacionamentoResult> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        repositorioEstacionamentoMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<Estacionamento>()), Times.Never);
        repositorioVagaMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<Vaga>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);

        const string mensagemEsperada = MensagensErro.UsuarioNaoIdentificado;
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
    public async Task Configurar_Deve_Retornar_Falha_Quando_Validacao_Falhar()
    {
        // Arrange
        ConfigurarEstacionamentoCommand command = new(nomeEstacionamentoPadrao, 0, 0, 0);

        ValidationResult validationComErros = new(new List<ValidationFailure>
        {
            new(nameof(command.QuantidadeVagas), "Quantidade de vagas deve ser maior que zero.")
        });

        validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationComErros);

        // Act
        Result<ConfigurarEstacionamentoResult> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        repositorioEstacionamentoMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<Estacionamento>()), Times.Never);
        repositorioVagaMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<Vaga>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Configurar_Deve_Retornar_Falha_Quando_Distribuicao_De_Vagas_For_Invalida()
    {
        // Arrange
        ConfigurarEstacionamentoCommand command = new(nomeEstacionamentoPadrao, 5, 2, 2);

        validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        Result<ConfigurarEstacionamentoResult> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        repositorioEstacionamentoMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<Estacionamento>()), Times.Never);
        repositorioVagaMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<Vaga>()), Times.Never);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);

        ImmutableList<string> mensagensDoResult = resultado.Errors
            .SelectMany(e => e.Reasons.OfType<Error>())
            .Select(r => r.Message)
            .ToImmutableList();

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
        Assert.IsTrue(mensagensDoResult.Count >= 1);
    }

    [TestMethod]
    public async Task Configurar_Deve_Retornar_Falha_Quando_Lancar_DbUpdateException()
    {
        // Arrange
        ConfigurarEstacionamentoCommand command = new(nomeEstacionamentoPadrao, quantidadeVagasPadrao,
            zonasTotaisPadrao, vagasPorZonaPadrao);

        validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Estacionamento estacionamentoMapeado = new() { Id = Guid.NewGuid(), Nome = nomeEstacionamentoPadrao };

        mapperMock
            .Setup(m => m.Map<Estacionamento>(command))
            .Returns(estacionamentoMapeado);

        repositorioEstacionamentoMock
            .Setup(r => r.CadastrarRegistroAsync(estacionamentoMapeado))
            .ThrowsAsync(new DbUpdateException("erro persistência"));

        unitOfWorkMock
            .Setup(u => u.RollbackAsync());

        // Act
        Result<ConfigurarEstacionamentoResult> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Never);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
    }

    [TestMethod]
    public async Task Configurar_Deve_Retornar_Falha_Quando_Lancar_Excecao()
    {
        // Arrange
        ConfigurarEstacionamentoCommand command = new(nomeEstacionamentoPadrao, quantidadeVagasPadrao,
            zonasTotaisPadrao, vagasPorZonaPadrao);

        validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Estacionamento estacionamentoMapeado = new() { Id = Guid.NewGuid(), Nome = nomeEstacionamentoPadrao };

        mapperMock
            .Setup(m => m.Map<Estacionamento>(command))
            .Returns(estacionamentoMapeado);

        repositorioEstacionamentoMock
            .Setup(r => r.CadastrarRegistroAsync(estacionamentoMapeado));

        repositorioVagaMock
            .Setup(r => r.CadastrarRegistroAsync(It.IsAny<Vaga>()));

        unitOfWorkMock
            .Setup(u => u.CommitAsync());

        cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("falha cache"));

        unitOfWorkMock
            .Setup(u => u.RollbackAsync());

        // Act
        Result<ConfigurarEstacionamentoResult> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsFailed);
    }
}
