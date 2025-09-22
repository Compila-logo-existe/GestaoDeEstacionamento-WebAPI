using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Handlers;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using Microsoft.Extensions.Caching.Distributed;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloRecepcaoCheckin;

[TestClass]
[TestCategory("Testes de Unidade de RegistrarEntradaCommandHandler")]
public class RegistrarEntradaCommandHandlerTestes
{
    private RegistrarEntradaCommandHandler handler = null!;

    private const string cpfPadrao = "12345678901";
    private const string placaPadrao = "ABC1D23";
    private const string nomeCompletoPadrao = "Fulano da Silva";
    private const string telefonePadrao = "11999998888";
    private const string modeloPadrao = "Aquele";
    private const string corPadrao = "Roxo";
    private const decimal valorDiariaPadrao = 50.0m;

    private readonly Guid tenantIdPadrao = Guid.NewGuid();
    private readonly Guid usuarioIdPadrao = Guid.NewGuid();
    private readonly Guid hospedeIdPadrao = Guid.NewGuid();
    private readonly Guid veiculoIdPadrao = Guid.NewGuid();
    private readonly Guid registroIdPadrao = Guid.NewGuid();

    private Mock<IValidator<RegistrarEntradaCommand>> validatorMock = null!;
    private Mock<IMapper> mapperMock = null!;
    private Mock<IRepositorioHospede> repositorioHospedeMock = null!;
    private Mock<IRepositorioVeiculo> repositorioVeiculoMock = null!;
    private Mock<IRepositorioRegistroEntrada> repositorioRegistroEntradaMock = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<IDistributedCache> cacheMock = null!;
    private Mock<IUnitOfWork> unitOfWorkMock = null!;
    private Mock<ILogger<RegistrarEntradaCommandHandler>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        validatorMock = new Mock<IValidator<RegistrarEntradaCommand>>();
        mapperMock = new Mock<IMapper>();
        repositorioHospedeMock = new Mock<IRepositorioHospede>();
        repositorioVeiculoMock = new Mock<IRepositorioVeiculo>();
        repositorioRegistroEntradaMock = new Mock<IRepositorioRegistroEntrada>();
        tenantProviderMock = new Mock<ITenantProvider>();
        cacheMock = new Mock<IDistributedCache>();
        unitOfWorkMock = new Mock<IUnitOfWork>();
        loggerMock = new Mock<ILogger<RegistrarEntradaCommandHandler>>();

        tenantProviderMock.SetupGet(p => p.TenantId)
            .Returns(tenantIdPadrao);
        tenantProviderMock.SetupGet(p => p.UsuarioId)
            .Returns(usuarioIdPadrao);

        handler = new RegistrarEntradaCommandHandler(
            validatorMock.Object,
            mapperMock.Object,
            repositorioHospedeMock.Object,
            repositorioVeiculoMock.Object,
            repositorioRegistroEntradaMock.Object,
            tenantProviderMock.Object,
            cacheMock.Object,
            unitOfWorkMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Handler_Deve_Registrar_Entrada_Com_Sucesso_Usando_Hospede_Existente_E_Veiculo_Existente()
    {
        // Arrange
        RegistrarEntradaCommand command = new(null, nomeCompletoPadrao, cpfPadrao,
            telefonePadrao, placaPadrao, modeloPadrao, corPadrao, valorDiariaPadrao,
            new List<string> { "Kebradu ali" }
        );

        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<RegistrarEntradaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Hospede hospedeExistente = new() { Id = hospedeIdPadrao };
        repositorioHospedeMock
            .Setup(r => r.SelecionarRegistroPorCPFAsync(cpfPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hospedeExistente);

        Veiculo veiculoExistente = new() { Id = veiculoIdPadrao };
        repositorioVeiculoMock
            .Setup(r => r.SelecionarRegistroPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculoExistente);

        repositorioRegistroEntradaMock
            .Setup(r => r.ExisteAberturaPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        RegistroEntrada registroMapeado = new() { Id = registroIdPadrao };
        mapperMock
            .Setup(m => m.Map<RegistroEntrada>(command))
            .Returns(registroMapeado);

        RegistrarEntradaResult resultadoMapeado = new(registroIdPadrao, 123, DateTime.UtcNow.ToString("O"));
        mapperMock
            .Setup(m => m.Map<RegistrarEntradaResult>(It.IsAny<RegistroEntrada>()))
            .Returns(resultadoMapeado);

        // Act
        Result<RegistrarEntradaResult> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        repositorioHospedeMock.Verify(r => r.SelecionarRegistroPorCPFAsync(cpfPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioVeiculoMock.Verify(r => r.SelecionarRegistroPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioRegistroEntradaMock.Verify(r => r.ExisteAberturaPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioRegistroEntradaMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<RegistroEntrada>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(resultadoMapeado, resultado.Value);
    }

    [TestMethod]
    public async Task Handler_Deve_Registrar_Entrada_Com_Sucesso_Criando_Hospede_E_Veiculo_Quando_Nao_Existirem()
    {
        // Arrange
        RegistrarEntradaCommand command = new(null, nomeCompletoPadrao, cpfPadrao,
            telefonePadrao, placaPadrao, modeloPadrao, corPadrao, valorDiariaPadrao,
            new List<string> { "Kebradu ali" }
        );

        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<RegistrarEntradaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        repositorioHospedeMock
            .Setup(r => r.SelecionarRegistroPorCPFAsync(cpfPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hospede?)null);

        Hospede hospedeCriado = new() { Id = hospedeIdPadrao };
        mapperMock
            .Setup(m => m.Map<Hospede>(command))
            .Returns(hospedeCriado);
        repositorioHospedeMock
            .Setup(r => r.CadastrarRegistroAsync(hospedeCriado))
            .Returns(Task.CompletedTask);

        repositorioVeiculoMock
            .Setup(r => r.SelecionarRegistroPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Veiculo?)null);

        Veiculo veiculoCriado = new() { Id = veiculoIdPadrao };
        mapperMock
            .Setup(m => m.Map<Veiculo>(command))
            .Returns(veiculoCriado);
        repositorioVeiculoMock
            .Setup(r => r.CadastrarRegistroAsync(veiculoCriado))
            .Returns(Task.CompletedTask);

        repositorioRegistroEntradaMock
            .Setup(r => r.ExisteAberturaPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        RegistroEntrada registroMapeado = new() { Id = registroIdPadrao };
        mapperMock
            .Setup(m => m.Map<RegistroEntrada>(command))
            .Returns(registroMapeado);

        RegistrarEntradaResult resultadoMapeado = new(registroIdPadrao, 7, DateTime.UtcNow.ToString("O"));
        mapperMock
            .Setup(m => m.Map<RegistrarEntradaResult>(It.IsAny<RegistroEntrada>()))
            .Returns(resultadoMapeado);

        // Act
        Result<RegistrarEntradaResult> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(It.IsAny<RegistrarEntradaCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        repositorioHospedeMock.Verify(r => r.CadastrarRegistroAsync(hospedeCriado), Times.Once);
        repositorioVeiculoMock.Verify(r => r.CadastrarRegistroAsync(veiculoCriado), Times.Once);
        repositorioRegistroEntradaMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<RegistroEntrada>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(resultadoMapeado, resultado.Value);
    }

    [TestMethod]
    public async Task Handler_Deve_Registrar_Entrada_Com_Sucesso_Usando_HospedeId_E_Veiculo_Existente()
    {
        // Arrange
        RegistrarEntradaCommand command = new(hospedeIdPadrao, null, null,
            null, placaPadrao, modeloPadrao, corPadrao, valorDiariaPadrao,
            new List<string> { "Kebradu ali" }
        );

        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<RegistrarEntradaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Hospede hospedeSelecionado = new() { Id = hospedeIdPadrao };
        repositorioHospedeMock
            .Setup(r => r.SelecionarRegistroPorIdAsync(hospedeIdPadrao))
            .ReturnsAsync(hospedeSelecionado);

        Veiculo veiculoExistente = new() { Id = veiculoIdPadrao };
        repositorioVeiculoMock
            .Setup(r => r.SelecionarRegistroPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculoExistente);

        repositorioRegistroEntradaMock
            .Setup(r => r.ExisteAberturaPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        RegistroEntrada registroMapeado = new() { Id = registroIdPadrao };
        mapperMock
            .Setup(m => m.Map<RegistroEntrada>(It.IsAny<RegistrarEntradaCommand>()))
            .Returns(registroMapeado);

        RegistrarEntradaResult resultadoMapeado = new(registroIdPadrao, 999, DateTime.UtcNow.ToString("O"));
        mapperMock
            .Setup(m => m.Map<RegistrarEntradaResult>(It.IsAny<RegistroEntrada>()))
            .Returns(resultadoMapeado);

        // Act
        Result<RegistrarEntradaResult> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(It.IsAny<RegistrarEntradaCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        repositorioHospedeMock.Verify(r => r.SelecionarRegistroPorIdAsync(hospedeIdPadrao), Times.Once);
        repositorioVeiculoMock.Verify(r => r.SelecionarRegistroPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioRegistroEntradaMock.Verify(r => r.ExisteAberturaPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioRegistroEntradaMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<RegistroEntrada>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(resultadoMapeado, resultado.Value);
    }
}
