using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Handlers;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloFaturamento;

[TestClass]
[TestCategory("Testes de Unidade de ObterValorAtualFaturamentoQueryHandler")]
public class ObterValorAtualFaturamentoQueryHandlerTestes
{
    private ObterValorAtualFaturamentoQueryHandler handler = null!;

    private const string nomeEstacionamentoPadrao = "Estacionamento Central";
    private const string placaPadrao = "ABC1D23";
    private const int numeroVagaPadrao = 10;
    private const int numeroTicketPadrao = 22;
    private const decimal valorDiariaPadrao = 50.0m;

    private readonly Guid tenantIdPadrao = Guid.NewGuid();
    private readonly Guid usuarioIdPadrao = Guid.NewGuid();
    private readonly Guid estacionamentoIdPadrao = Guid.NewGuid();
    private readonly Guid vagaIdPadrao = Guid.NewGuid();
    private readonly Guid registroIdPadrao = Guid.NewGuid();
    private readonly Guid veiculoIdPadrao = Guid.NewGuid();

    private Mock<IValidator<ObterValorAtualFaturamentoQuery>> validatorMock = null!;
    private Mock<IMapper> mapperMock = null!;
    private Mock<IRepositorioRegistroEntrada> repositorioRegistroEntradaMock = null!;
    private Mock<IRepositorioEstacionamento> repositorioEstacionamentoMock = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<ILogger<ObterValorAtualFaturamentoQueryHandler>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        validatorMock = new Mock<IValidator<ObterValorAtualFaturamentoQuery>>();
        mapperMock = new Mock<IMapper>();
        repositorioRegistroEntradaMock = new Mock<IRepositorioRegistroEntrada>();
        repositorioEstacionamentoMock = new Mock<IRepositorioEstacionamento>();
        tenantProviderMock = new Mock<ITenantProvider>();
        loggerMock = new Mock<ILogger<ObterValorAtualFaturamentoQueryHandler>>();

        tenantProviderMock.SetupGet(p => p.TenantId)
            .Returns(tenantIdPadrao);
        tenantProviderMock.SetupGet(p => p.UsuarioId)
            .Returns(usuarioIdPadrao);

        handler = new ObterValorAtualFaturamentoQueryHandler(
            validatorMock.Object,
            mapperMock.Object,
            repositorioRegistroEntradaMock.Object,
            repositorioEstacionamentoMock.Object,
            tenantProviderMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Query_Deve_Valor_Faturamento_Com_Sucesso_Usando_Nome_E_Numero_Ticket()
    {
        // Arrange
        ObterValorAtualFaturamentoQuery query = new(null, nomeEstacionamentoPadrao!
            , numeroTicketPadrao, null);

        validatorMock
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Ticket? ticket = new();
        RegistroEntrada registroEntradaMapeado = new() { Id = registroIdPadrao, Ticket = ticket };
        ticket.RegistroEntrada = registroEntradaMapeado;
        registroEntradaMapeado.GerarNovoFaturamento(valorDiariaPadrao);

        repositorioRegistroEntradaMock
            .Setup(r => r.SelecionarAberturaPorNumeroDoTicketAsync(numeroTicketPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registroEntradaMapeado);

        Estacionamento estacionamentoMapeado = new() { Id = estacionamentoIdPadrao, Nome = nomeEstacionamentoPadrao };
        Vaga vaga = new()
        {
            Id = vagaIdPadrao,
            Estacionamento = estacionamentoMapeado,
            Numero = numeroVagaPadrao,
            Zona = ZonaEstacionamento.A
        };
        Veiculo veiculo = new() { Id = veiculoIdPadrao, Placa = placaPadrao, Vaga = vaga };
        registroEntradaMapeado.AderirVeiculo(veiculo);

        repositorioEstacionamentoMock
            .Setup(r => r.SelecionarRegistroPorNome(nomeEstacionamentoPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(estacionamentoMapeado);

        const int numeroDeDiariasEsperado = 5;
        const decimal valorDaDiariaEsperado = valorDiariaPadrao;
        const decimal valorTotalEsperado = numeroDeDiariasEsperado * valorDaDiariaEsperado;

        ObterValorAtualFaturamentoResult resultadoMapeado = new(numeroDeDiariasEsperado, valorDaDiariaEsperado, valorTotalEsperado);
        mapperMock
            .Setup(m => m.Map<ObterValorAtualFaturamentoResult>(It.IsAny<(int, decimal, decimal)>()))
            .Returns(resultadoMapeado);

        // Act
        Result<ObterValorAtualFaturamentoResult> resultado = await handler.Handle(query, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);

        repositorioRegistroEntradaMock.Verify(r =>
            r.SelecionarAberturaPorNumeroDoTicketAsync(numeroTicketPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);

        repositorioEstacionamentoMock.Verify(r =>
            r.SelecionarRegistroPorNome(nomeEstacionamentoPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);

        mapperMock.Verify(m => m.Map<ObterValorAtualFaturamentoResult>(
                It.IsAny<(int, decimal, decimal)>()), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreSame(resultadoMapeado, resultado.Value);
    }

    [TestMethod]
    public async Task Query_Deve_Valor_Faturamento_Com_Sucesso_Usando_Id_E_Numero_Placa()
    {
        // Arrange
        ObterValorAtualFaturamentoQuery query = new(estacionamentoIdPadrao, null!
            , null, placaPadrao);

        validatorMock
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Ticket? ticket = new();
        RegistroEntrada registroEntradaMapeado = new() { Id = registroIdPadrao, Ticket = ticket };
        ticket.RegistroEntrada = registroEntradaMapeado;
        registroEntradaMapeado.GerarNovoFaturamento(valorDiariaPadrao);

        repositorioRegistroEntradaMock
            .Setup(r => r.SelecionarAberturaPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registroEntradaMapeado);

        Estacionamento estacionamentoMapeado = new() { Id = estacionamentoIdPadrao, Nome = nomeEstacionamentoPadrao };
        Vaga vaga = new()
        {
            Id = vagaIdPadrao,
            Estacionamento = estacionamentoMapeado,
            Numero = numeroVagaPadrao,
            Zona = ZonaEstacionamento.A
        };
        Veiculo veiculo = new() { Id = veiculoIdPadrao, Placa = placaPadrao, Vaga = vaga };
        registroEntradaMapeado.AderirVeiculo(veiculo);

        repositorioEstacionamentoMock
            .Setup(r => r.SelecionarRegistroPorIdAsync(estacionamentoIdPadrao))
            .ReturnsAsync(estacionamentoMapeado);

        const int numeroDeDiariasEsperado = 5;
        const decimal valorDaDiariaEsperado = valorDiariaPadrao;
        const decimal valorTotalEsperado = numeroDeDiariasEsperado * valorDaDiariaEsperado;

        ObterValorAtualFaturamentoResult resultadoMapeado = new(numeroDeDiariasEsperado, valorDaDiariaEsperado, valorTotalEsperado);
        mapperMock
            .Setup(m => m.Map<ObterValorAtualFaturamentoResult>(It.IsAny<(int, decimal, decimal)>()))
            .Returns(resultadoMapeado);

        // Act
        Result<ObterValorAtualFaturamentoResult> resultado = await handler.Handle(query, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);

        repositorioRegistroEntradaMock.Verify(r =>
            r.SelecionarAberturaPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);

        repositorioEstacionamentoMock.Verify(r =>
            r.SelecionarRegistroPorIdAsync(estacionamentoIdPadrao), Times.Once);

        mapperMock.Verify(m => m.Map<ObterValorAtualFaturamentoResult>(
                It.IsAny<(int, decimal, decimal)>()), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreSame(resultadoMapeado, resultado.Value);
    }
}
