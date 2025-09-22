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
[TestCategory("Testes de Unidade de RegistrarSaidaCommandHandler")]
public class RegistrarSaidaCommandHandlerTestes
{
    private RegistrarSaidaCommandHandler handler = null!;

    private const string cpfPadrao = "12345678901";
    private const string placaPadrao = "ABC1D23";
    private const int numeroTicketPadrao = 22;
    private const decimal valorDiariaPadrao = 50.0m;

    private readonly Guid tenantIdPadrao = Guid.NewGuid();
    private readonly Guid usuarioIdPadrao = Guid.NewGuid();
    private readonly Guid hospedeIdPadrao = Guid.NewGuid();
    private readonly Guid veiculoIdPadrao = Guid.NewGuid();
    private readonly Guid registroIdPadrao = Guid.NewGuid();

    private Mock<IValidator<RegistrarSaidaCommand>> validatorMock = null!;
    private Mock<IMapper> mapperMock = null!;
    private Mock<IRepositorioHospede> repositorioHospedeMock = null!;
    private Mock<IRepositorioVeiculo> repositorioVeiculoMock = null!;
    private Mock<IRepositorioRegistroEntrada> repositorioRegistroEntradaMock = null!;
    private Mock<IRepositorioRegistroSaida> repositorioRegistroSaidaMock = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<IDistributedCache> cacheMock = null!;
    private Mock<IUnitOfWork> unitOfWorkMock = null!;
    private Mock<ILogger<RegistrarSaidaCommandHandler>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        validatorMock = new Mock<IValidator<RegistrarSaidaCommand>>();
        mapperMock = new Mock<IMapper>();
        repositorioHospedeMock = new Mock<IRepositorioHospede>();
        repositorioVeiculoMock = new Mock<IRepositorioVeiculo>();
        repositorioRegistroEntradaMock = new Mock<IRepositorioRegistroEntrada>();
        repositorioRegistroSaidaMock = new Mock<IRepositorioRegistroSaida>();
        tenantProviderMock = new Mock<ITenantProvider>();
        cacheMock = new Mock<IDistributedCache>();
        unitOfWorkMock = new Mock<IUnitOfWork>();
        loggerMock = new Mock<ILogger<RegistrarSaidaCommandHandler>>();

        tenantProviderMock.SetupGet(p => p.TenantId)
            .Returns(tenantIdPadrao);
        tenantProviderMock.SetupGet(p => p.UsuarioId)
            .Returns(usuarioIdPadrao);

        handler = new RegistrarSaidaCommandHandler(
            validatorMock.Object,
            mapperMock.Object,
            repositorioHospedeMock.Object,
            repositorioVeiculoMock.Object,
            repositorioRegistroEntradaMock.Object,
            repositorioRegistroSaidaMock.Object,
            tenantProviderMock.Object,
            cacheMock.Object,
            unitOfWorkMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Handler_Deve_Registrar_Saida_Com_Sucesso_Usando_CPF_e_Placa()
    {
        // Arrange
        RegistrarSaidaCommand command = new(null, cpfPadrao, numeroTicketPadrao,
            null, placaPadrao
        );

        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<RegistrarSaidaCommand>(), It.IsAny<CancellationToken>()))
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
            .Setup(r => r.ExisteAberturaPorPlacaAsync(veiculoExistente.Placa, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Ticket? ticket = new();
        RegistroEntrada registroEntradaMapeado = new() { Id = registroIdPadrao, Ticket = ticket };
        ticket.RegistroEntrada = registroEntradaMapeado;
        registroEntradaMapeado.GerarNovoFaturamento(valorDiariaPadrao);

        repositorioRegistroEntradaMock
            .Setup(r => r.SelecionarAberturaPorNumeroDoTicketAsync(numeroTicketPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registroEntradaMapeado);

        RegistroSaida registroSaidaMapeado = new() { Id = registroIdPadrao };
        mapperMock
            .Setup(m => m.Map<RegistroSaida>(It.IsAny<RegistrarSaidaCommand>()))
            .Returns(registroSaidaMapeado);

        RegistrarSaidaResult resultadoMapeado = new(DateTime.UtcNow.ToString("O"));
        mapperMock
            .Setup(m => m.Map<RegistrarSaidaResult>(It.IsAny<RegistroSaida>()))
            .Returns(resultadoMapeado);

        // Act
        Result<RegistrarSaidaResult> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(It.IsAny<RegistrarSaidaCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        repositorioHospedeMock.Verify(r => r.SelecionarRegistroPorCPFAsync(cpfPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioVeiculoMock.Verify(r => r.SelecionarRegistroPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioRegistroEntradaMock.Verify(r => r.ExisteAberturaPorPlacaAsync(veiculoExistente.Placa, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioRegistroEntradaMock.Verify(r => r.SelecionarAberturaPorNumeroDoTicketAsync(numeroTicketPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioRegistroSaidaMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<RegistroSaida>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(resultadoMapeado, resultado.Value);
    }

    [TestMethod]
    public async Task Handler_Deve_Registrar_Saida_Com_Sucesso_Usando_Ids()
    {
        // Arrange
        RegistrarSaidaCommand command = new(hospedeIdPadrao, null, numeroTicketPadrao,
            veiculoIdPadrao, null
        );

        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<RegistrarSaidaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Hospede hospedeExistente = new() { Id = hospedeIdPadrao };
        repositorioHospedeMock
            .Setup(r => r.SelecionarRegistroPorIdAsync(hospedeIdPadrao))
            .ReturnsAsync(hospedeExistente);

        Veiculo veiculoExistente = new() { Id = veiculoIdPadrao };
        repositorioVeiculoMock
            .Setup(r => r.SelecionarRegistroPorIdAsync(veiculoIdPadrao))
            .ReturnsAsync(veiculoExistente);

        repositorioRegistroEntradaMock
            .Setup(r => r.ExisteAberturaPorPlacaAsync(veiculoExistente.Placa, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Ticket? ticket = new();
        RegistroEntrada registroEntradaMapeado = new() { Id = registroIdPadrao, Ticket = ticket };
        ticket.RegistroEntrada = registroEntradaMapeado;
        registroEntradaMapeado.GerarNovoFaturamento(valorDiariaPadrao);

        repositorioRegistroEntradaMock
            .Setup(r => r.SelecionarAberturaPorNumeroDoTicketAsync(numeroTicketPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registroEntradaMapeado);

        RegistroSaida registroSaidaMapeado = new() { Id = registroIdPadrao };
        mapperMock
            .Setup(m => m.Map<RegistroSaida>(It.IsAny<RegistrarSaidaCommand>()))
            .Returns(registroSaidaMapeado);

        RegistrarSaidaResult resultadoMapeado = new(DateTime.UtcNow.ToString("O"));
        mapperMock
            .Setup(m => m.Map<RegistrarSaidaResult>(It.IsAny<RegistroSaida>()))
            .Returns(resultadoMapeado);

        // Act
        Result<RegistrarSaidaResult> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(It.IsAny<RegistrarSaidaCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        repositorioHospedeMock.Verify(r => r.SelecionarRegistroPorIdAsync(hospedeIdPadrao), Times.Once);
        repositorioVeiculoMock.Verify(r => r.SelecionarRegistroPorIdAsync(veiculoIdPadrao), Times.Once);
        repositorioRegistroEntradaMock.Verify(r => r.ExisteAberturaPorPlacaAsync(veiculoExistente.Placa, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioRegistroEntradaMock.Verify(r => r.SelecionarAberturaPorNumeroDoTicketAsync(numeroTicketPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioRegistroSaidaMock.Verify(r => r.CadastrarRegistroAsync(It.IsAny<RegistroSaida>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreEqual(resultadoMapeado, resultado.Value);
    }
}
