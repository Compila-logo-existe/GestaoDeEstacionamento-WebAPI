using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Handlers;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using Microsoft.Extensions.Caching.Distributed;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloEstacionamento;

[TestClass]
[TestCategory("Testes de Unidade de OcuparVagaCommandHandler")]
public class OcuparVagaCommandHandlerTestes
{
    private OcuparVagaCommandHandler handler = null!;

    private const string nomeEstacionamentoPadrao = "Estacionamento Central";
    private const string placaPadrao = "ABC1D23";
    private const string zonaPadrao = "A";
    private const int numeroVagaPadrao = 10;

    private readonly Guid tenantIdPadrao = Guid.NewGuid();
    private readonly Guid usuarioIdPadrao = Guid.NewGuid();
    private readonly Guid estacionamentoIdPadrao = Guid.NewGuid();
    private readonly Guid vagaIdPadrao = Guid.NewGuid();
    private readonly Guid veiculoIdPadrao = Guid.NewGuid();

    private Mock<IValidator<OcuparVagaCommand>> validatorMock = null!;
    private Mock<IMapper> mapperMock = null!;
    private Mock<IRepositorioEstacionamento> repositorioEstacionamentoMock = null!;
    private Mock<IRepositorioRegistroEntrada> repositorioRegistroEntradaMock = null!;
    private Mock<IRepositorioVaga> repositorioVagaMock = null!;
    private Mock<IRepositorioVeiculo> repositorioVeiculoMock = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<IDistributedCache> cacheMock = null!;
    private Mock<IUnitOfWork> unitOfWorkMock = null!;
    private Mock<ILogger<OcuparVagaCommandHandler>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        validatorMock = new Mock<IValidator<OcuparVagaCommand>>();
        mapperMock = new Mock<IMapper>();
        repositorioEstacionamentoMock = new Mock<IRepositorioEstacionamento>();
        repositorioRegistroEntradaMock = new Mock<IRepositorioRegistroEntrada>();
        repositorioVagaMock = new Mock<IRepositorioVaga>();
        repositorioVeiculoMock = new Mock<IRepositorioVeiculo>();
        tenantProviderMock = new Mock<ITenantProvider>();
        cacheMock = new Mock<IDistributedCache>();
        unitOfWorkMock = new Mock<IUnitOfWork>();
        loggerMock = new Mock<ILogger<OcuparVagaCommandHandler>>();

        tenantProviderMock.SetupGet(p => p.TenantId)
            .Returns(tenantIdPadrao);
        tenantProviderMock.SetupGet(p => p.UsuarioId)
            .Returns(usuarioIdPadrao);

        handler = new OcuparVagaCommandHandler(
            validatorMock.Object,
            mapperMock.Object,
            repositorioEstacionamentoMock.Object,
            repositorioRegistroEntradaMock.Object,
            repositorioVagaMock.Object,
            repositorioVeiculoMock.Object,
            tenantProviderMock.Object,
            cacheMock.Object,
            unitOfWorkMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Ocupar_Deve_Retornar_Sucesso_Usando_Ids()
    {
        // Arrange
        OcuparVagaCommand command = new(estacionamentoIdPadrao, null,
            vagaIdPadrao, null, null, veiculoIdPadrao, null
        );

        validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Estacionamento estacionamento = new() { Id = estacionamentoIdPadrao, Nome = nomeEstacionamentoPadrao };
        Vaga vaga = new()
        {
            Id = vagaIdPadrao,
            Estacionamento = estacionamento,
            Numero = numeroVagaPadrao,
            Zona = ZonaEstacionamento.A
        };
        Veiculo veiculo = new() { Id = veiculoIdPadrao, Placa = placaPadrao };

        repositorioEstacionamentoMock
            .Setup(r => r.SelecionarRegistroPorIdAsync(estacionamentoIdPadrao))
            .ReturnsAsync(estacionamento);

        repositorioVagaMock
            .Setup(r => r.SelecionarRegistroPorIdAsync(vagaIdPadrao))
            .ReturnsAsync(vaga);

        repositorioVeiculoMock
            .Setup(r => r.SelecionarRegistroPorIdAsync(veiculoIdPadrao))
            .ReturnsAsync(veiculo);

        repositorioRegistroEntradaMock
            .Setup(r => r.ExisteAberturaPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        unitOfWorkMock
            .Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        OcuparVagaResult resultadoMapeado =
            new(new OcuparVagaDto(true, nomeEstacionamentoPadrao, $"{zonaPadrao}-{numeroVagaPadrao}"));

        mapperMock
            .Setup(m => m.Map<(Estacionamento, Vaga), OcuparVagaResult>(It.Is<(Estacionamento, Vaga)>(t => t.Item1 == estacionamento && t.Item2 == vaga)))
            .Returns(resultadoMapeado);

        // Act
        Result<OcuparVagaResult> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        repositorioEstacionamentoMock.Verify(r => r.SelecionarRegistroPorIdAsync(estacionamentoIdPadrao), Times.Once);
        repositorioVagaMock.Verify(r => r.SelecionarRegistroPorIdAsync(vagaIdPadrao), Times.Once);
        repositorioVeiculoMock.Verify(r => r.SelecionarRegistroPorIdAsync(veiculoIdPadrao), Times.Once);
        repositorioRegistroEntradaMock.Verify(r => r.ExisteAberturaPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.IsTrue(resultado.Value.Vaga.Estacionou);
        Assert.AreEqual(nomeEstacionamentoPadrao, resultado.Value.Vaga.EstacionamentoNome);
        Assert.AreEqual($"{zonaPadrao}-{numeroVagaPadrao}", resultado.Value.Vaga.IdentificacaoVaga);
    }

    [TestMethod]
    public async Task Ocupar_Deve_Retornar_Sucesso_Usando_Nome_VagaNumero_VagaZona_Placa()
    {
        // Arrange
        OcuparVagaCommand command = new(
            null, nomeEstacionamentoPadrao,
            null, numeroVagaPadrao, zonaPadrao,
            null, placaPadrao
        );

        validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Estacionamento estacionamento = new() { Id = estacionamentoIdPadrao, Nome = nomeEstacionamentoPadrao };
        Vaga vaga = new()
        {
            Id = vagaIdPadrao,
            Estacionamento = estacionamento,
            Numero = numeroVagaPadrao,
            Zona = ZonaEstacionamento.A
        };
        Veiculo veiculo = new() { Id = veiculoIdPadrao, Placa = placaPadrao };

        repositorioEstacionamentoMock
            .Setup(r => r.SelecionarRegistroPorNome(nomeEstacionamentoPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estacionamento);

        repositorioVagaMock
            .Setup(r => r.SelecionarRegistroPorDadosAsync(
                numeroVagaPadrao,
                ZonaEstacionamento.A,
                estacionamentoIdPadrao,
                tenantIdPadrao,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);

        repositorioVeiculoMock
            .Setup(r => r.SelecionarRegistroPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculo);

        repositorioRegistroEntradaMock
            .Setup(r => r.ExisteAberturaPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        unitOfWorkMock
            .Setup(u => u.CommitAsync())
            .Returns(Task.CompletedTask);

        cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        OcuparVagaResult resultadoMapeado =
            new(new OcuparVagaDto(true, nomeEstacionamentoPadrao, $"{zonaPadrao}-{numeroVagaPadrao}"));

        mapperMock
            .Setup(m => m.Map<(Estacionamento, Vaga), OcuparVagaResult>(It.Is<(Estacionamento, Vaga)>(t => t.Item1 == estacionamento && t.Item2 == vaga)))
            .Returns(resultadoMapeado);

        // Act
        Result<OcuparVagaResult> resultado = await handler.Handle(command, CancellationToken.None);

        // Assert
        repositorioEstacionamentoMock.Verify(r => r.SelecionarRegistroPorNome(nomeEstacionamentoPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioVagaMock.Verify(r => r.SelecionarRegistroPorDadosAsync(numeroVagaPadrao, ZonaEstacionamento.A, estacionamentoIdPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioVeiculoMock.Verify(r => r.SelecionarRegistroPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioRegistroEntradaMock.Verify(r => r.ExisteAberturaPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.IsTrue(resultado.Value.Vaga.Estacionou);
        Assert.AreEqual(nomeEstacionamentoPadrao, resultado.Value.Vaga.EstacionamentoNome);
        Assert.AreEqual($"{zonaPadrao}-{numeroVagaPadrao}", resultado.Value.Vaga.IdentificacaoVaga);
    }
}
