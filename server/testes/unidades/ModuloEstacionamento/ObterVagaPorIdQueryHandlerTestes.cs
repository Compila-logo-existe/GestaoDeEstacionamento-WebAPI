using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Handlers;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloEstacionamento;

[TestClass]
[TestCategory("Testes de Unidade de ObterVagaPorIdQueryHandler")]
public class ObterVagaPorIdQueryHandlerTestes
{
    private ObterVagaPorIdQueryHandler handler = null!;

    private const string nomeEstacionamentoPadrao = "Estacionamento Central";
    private const string placaPadrao = "ABC1D23";
    private const string zonaPadrao = "A";
    private const int numeroVagaPadrao = 10;

    private readonly Guid tenantIdPadrao = Guid.NewGuid();
    private readonly Guid usuarioIdPadrao = Guid.NewGuid();
    private readonly Guid estacionamentoIdPadrao = Guid.NewGuid();
    private readonly Guid vagaIdPadrao = Guid.NewGuid();

    private Mock<IValidator<ObterVagaPorIdQuery>> validatorMock = null!;
    private Mock<IMapper> mapperMock = null!;
    private Mock<IRepositorioEstacionamento> repositorioEstacionamentoMock = null!;
    private Mock<IRepositorioVaga> repositorioVagaMock = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<ILogger<ObterVagaPorIdQueryHandler>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        validatorMock = new Mock<IValidator<ObterVagaPorIdQuery>>();
        mapperMock = new Mock<IMapper>();
        repositorioEstacionamentoMock = new Mock<IRepositorioEstacionamento>();
        repositorioVagaMock = new Mock<IRepositorioVaga>();
        tenantProviderMock = new Mock<ITenantProvider>();
        loggerMock = new Mock<ILogger<ObterVagaPorIdQueryHandler>>();

        tenantProviderMock.SetupGet(p => p.TenantId).Returns(tenantIdPadrao);
        tenantProviderMock.SetupGet(p => p.UsuarioId).Returns(usuarioIdPadrao);

        handler = new ObterVagaPorIdQueryHandler(
            validatorMock.Object,
            mapperMock.Object,
            repositorioEstacionamentoMock.Object,
            repositorioVagaMock.Object,
            tenantProviderMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Query_Deve_Obter_Vaga_Com_Sucesso_Usando_EstacionamentoId_E_VagaId()
    {
        // Arrange
        ObterVagaPorIdQuery query = new(estacionamentoIdPadrao, null, vagaIdPadrao, null, null);

        validatorMock
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Estacionamento estacionamento = new() { Id = estacionamentoIdPadrao, Nome = nomeEstacionamentoPadrao };

        Vaga vaga = new()
        {
            Id = vagaIdPadrao,
            Estacionamento = estacionamento,
            Numero = numeroVagaPadrao,
            Zona = ZonaEstacionamento.A
        };

        repositorioEstacionamentoMock
            .Setup(r => r.SelecionarRegistroPorIdAsync(estacionamentoIdPadrao))
            .ReturnsAsync(estacionamento);

        repositorioVagaMock
            .Setup(r => r.SelecionarRegistroPorIdAsync(vagaIdPadrao))
            .ReturnsAsync(vaga);

        ObterStatusVagasDto vagaDto = new(
            vaga.Id,
            vaga.Numero,
            vaga.Zona,
            vaga.Status,
            vaga.Veiculo?.Placa
        );
        ObterVagaPorIdResult resultadoMapeado = new(vagaDto);

        mapperMock
            .Setup(m => m.Map<ObterVagaPorIdResult>(It.IsAny<Vaga>()))
            .Returns(resultadoMapeado);

        // Act
        Result<ObterVagaPorIdResult> resultado = await handler.Handle(query, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        repositorioEstacionamentoMock.Verify(r => r.SelecionarRegistroPorIdAsync(estacionamentoIdPadrao), Times.Once);
        repositorioVagaMock.Verify(r => r.SelecionarRegistroPorIdAsync(vagaIdPadrao), Times.Once);
        mapperMock.Verify(m => m.Map<ObterVagaPorIdResult>(vaga), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreSame(resultadoMapeado, resultado.Value);
    }

    [TestMethod]
    public async Task Query_Deve_Obter_Vaga_Com_Sucesso_Usando_EstacionamentoNome_E_VagaNumeroZona()
    {
        // Arrange
        const string zonaInformada = "a";

        ObterVagaPorIdQuery query = new(null, nomeEstacionamentoPadrao, null, numeroVagaPadrao, zonaInformada);

        validatorMock
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Estacionamento estacionamento = new() { Id = estacionamentoIdPadrao, Nome = nomeEstacionamentoPadrao };

        Vaga vaga = new()
        {
            Id = vagaIdPadrao,
            Estacionamento = estacionamento,
            Numero = numeroVagaPadrao,
            Zona = ZonaEstacionamento.A
        };

        repositorioEstacionamentoMock
            .Setup(r => r.SelecionarRegistroPorNome(nomeEstacionamentoPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estacionamento);

        repositorioVagaMock
            .Setup(r => r.SelecionarRegistroPorDadosAsync(numeroVagaPadrao, ZonaEstacionamento.A,
            estacionamentoIdPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vaga);

        ObterStatusVagasDto vagaDto = new(
            vaga.Id,
            vaga.Numero,
            vaga.Zona,
            vaga.Status,
            vaga.Veiculo?.Placa
        );
        ObterVagaPorIdResult resultadoMapeado = new(vagaDto);

        mapperMock
            .Setup(m => m.Map<ObterVagaPorIdResult>(It.IsAny<Vaga>()))
            .Returns(resultadoMapeado);

        // Act
        Result<ObterVagaPorIdResult> resultado = await handler.Handle(query, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        repositorioEstacionamentoMock.Verify(r => r.SelecionarRegistroPorNome(nomeEstacionamentoPadrao,
            tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioVagaMock.Verify(r => r.SelecionarRegistroPorDadosAsync(numeroVagaPadrao, ZonaEstacionamento.A,
            estacionamentoIdPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        mapperMock.Verify(m => m.Map<ObterVagaPorIdResult>(vaga), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreSame(resultadoMapeado, resultado.Value);
    }
}
