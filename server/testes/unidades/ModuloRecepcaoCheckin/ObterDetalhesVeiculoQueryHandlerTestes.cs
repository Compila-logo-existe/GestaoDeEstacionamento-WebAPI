using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Handlers;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloRecepcaoCheckin;

[TestClass]
[TestCategory("Testes de Unidade de ObterDetalhesVeiculoQueryHandler")]
public class ObterDetalhesVeiculoQueryHandlerTestes
{
    private ObterDetalhesVeiculoQueryHandler handler = null!;

    private const string nomeEstacionamentoPadrao = "Estacionamento Central";
    private const int numeroTicketPadrao = 22;
    private const string placaPadrao = "ABC1D23";
    private const string modeloPadrao = "Aquele";
    private const string corPadrao = "Roxo";
    private const string nomeCompletoPadrao = "Fulano da Silva";

    private readonly Guid tenantIdPadrao = Guid.NewGuid();
    private readonly Guid usuarioIdPadrao = Guid.NewGuid();
    private readonly Guid estacionamentoIdPadrao = Guid.NewGuid();
    private readonly Guid veiculoIdPadrao = Guid.NewGuid();
    private readonly Guid registroIdPadrao = Guid.NewGuid();

    private Mock<IValidator<ObterDetalhesVeiculoQuery>> validatorMock = null!;
    private Mock<IMapper> mapperMock = null!;
    private Mock<IRepositorioVeiculo> repositorioVeiculoMock = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<ILogger<ObterDetalhesVeiculoQueryHandler>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        validatorMock = new Mock<IValidator<ObterDetalhesVeiculoQuery>>();
        mapperMock = new Mock<IMapper>();
        repositorioVeiculoMock = new Mock<IRepositorioVeiculo>();
        tenantProviderMock = new Mock<ITenantProvider>();
        loggerMock = new Mock<ILogger<ObterDetalhesVeiculoQueryHandler>>();

        tenantProviderMock.SetupGet(p => p.TenantId)
            .Returns(tenantIdPadrao);
        tenantProviderMock.SetupGet(p => p.UsuarioId)
            .Returns(usuarioIdPadrao);

        handler = new ObterDetalhesVeiculoQueryHandler(
            validatorMock.Object,
            mapperMock.Object,
            repositorioVeiculoMock.Object,
            tenantProviderMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Query_Deve_Obter_Detalhes_Veiculo_Com_Sucesso_Usando_Placa()
    {
        // Arrange
        ObterDetalhesVeiculoQuery query = new(null, placaPadrao);

        validatorMock
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Veiculo veiculoExistente = new() { Id = veiculoIdPadrao };
        repositorioVeiculoMock
            .Setup(r => r.SelecionarRegistroPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculoExistente);

        ObterDetalhesVeiculoResult resultadoMapeado = new(
            veiculoExistente.Id,
            placaPadrao,
            modeloPadrao,
            corPadrao,
            new List<string>(),
            nomeCompletoPadrao
        );

        mapperMock
            .Setup(m => m.Map<ObterDetalhesVeiculoResult>(It.IsAny<Veiculo>()))
            .Returns(resultadoMapeado);

        // Act
        Result<ObterDetalhesVeiculoResult> resultado = await handler.Handle(query, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        repositorioVeiculoMock.Verify(r => r.SelecionarRegistroPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        mapperMock.Verify(m => m.Map<ObterDetalhesVeiculoResult>(veiculoExistente), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreSame(resultadoMapeado, resultado.Value);
    }

    [TestMethod]
    public async Task Query_Deve_Obter_Detalhes_Veiculo_Com_Sucesso_Id()
    {
        // Arrange
        ObterDetalhesVeiculoQuery query = new(veiculoIdPadrao, null);

        validatorMock
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Veiculo veiculoExistente = new() { Id = veiculoIdPadrao };
        repositorioVeiculoMock
            .Setup(r => r.SelecionarRegistroPorIdAsync(veiculoIdPadrao))
            .ReturnsAsync(veiculoExistente);

        ObterDetalhesVeiculoResult resultadoMapeado = new(
            veiculoExistente.Id,
            placaPadrao,
            modeloPadrao,
            corPadrao,
            new List<string>(),
            nomeCompletoPadrao
        );

        mapperMock
            .Setup(m => m.Map<ObterDetalhesVeiculoResult>(It.IsAny<Veiculo>()))
            .Returns(resultadoMapeado);

        // Act
        Result<ObterDetalhesVeiculoResult> resultado = await handler.Handle(query, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        repositorioVeiculoMock.Verify(r => r.SelecionarRegistroPorIdAsync(veiculoIdPadrao), Times.Once);
        mapperMock.Verify(m => m.Map<ObterDetalhesVeiculoResult>(veiculoExistente), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreSame(resultadoMapeado, resultado.Value);
    }
}
