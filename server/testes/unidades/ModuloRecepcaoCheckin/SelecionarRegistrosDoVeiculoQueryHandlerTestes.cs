using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Handlers;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloRecepcaoCheckin;

[TestClass]
[TestCategory("Testes de Unidade de SelecionarRegistrosDoVeiculoQueryHandler")]
public class SelecionarRegistrosDoVeiculoQueryHandlerTestes
{
    private SelecionarRegistrosDoVeiculoQueryHandler handler = null!;

    private const string nomeEstacionamentoPadrao = "Estacionamento Central";
    private const int numeroTicketPadrao = 22;
    private const string placaPadrao = "ABC1D23";
    private const string nomeCompletoPadrao = "Fulano da Silva";

    private readonly Guid tenantIdPadrao = Guid.NewGuid();
    private readonly Guid usuarioIdPadrao = Guid.NewGuid();
    private readonly Guid estacionamentoIdPadrao = Guid.NewGuid();
    private readonly Guid veiculoIdPadrao = Guid.NewGuid();
    private readonly Guid registroIdPadrao = Guid.NewGuid();

    private Mock<IValidator<SelecionarRegistrosDoVeiculoQuery>> validatorMock = null!;
    private Mock<IMapper> mapperMock = null!;
    private Mock<IRepositorioVeiculo> repositorioVeiculoMock = null!;
    private Mock<IRepositorioRegistroEntrada> repositorioRegistroEntradaMock = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<IDistributedCache> cacheMock = null!;
    private Mock<ILogger<SelecionarRegistrosDoVeiculoQueryHandler>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        validatorMock = new Mock<IValidator<SelecionarRegistrosDoVeiculoQuery>>();
        mapperMock = new Mock<IMapper>();
        repositorioVeiculoMock = new Mock<IRepositorioVeiculo>();
        repositorioRegistroEntradaMock = new Mock<IRepositorioRegistroEntrada>();
        tenantProviderMock = new Mock<ITenantProvider>();
        cacheMock = new Mock<IDistributedCache>();
        loggerMock = new Mock<ILogger<SelecionarRegistrosDoVeiculoQueryHandler>>();

        tenantProviderMock.SetupGet(p => p.TenantId)
            .Returns(tenantIdPadrao);
        tenantProviderMock.SetupGet(p => p.UsuarioId)
            .Returns(usuarioIdPadrao);

        handler = new SelecionarRegistrosDoVeiculoQueryHandler(
            validatorMock.Object,
            mapperMock.Object,
            repositorioRegistroEntradaMock.Object,
            repositorioVeiculoMock.Object,
            tenantProviderMock.Object,
            cacheMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Query_Deve_Registros_Entradas_Do_Veiculo_Com_Sucesso_Usando_Placa_Veiculo()
    {
        // Arrange
        const int quantidade = 0;
        SelecionarRegistrosDoVeiculoQuery query = new(quantidade, null, placaPadrao);

        validatorMock
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Estacionamento estacionamento = new() { Id = estacionamentoIdPadrao, Nome = nomeEstacionamentoPadrao };

        string placaPadronizada = Padronizador.PadronizarPlaca(placaPadrao);

        Veiculo veiculoExistente = new() { Id = veiculoIdPadrao };

        repositorioVeiculoMock
            .Setup(r => r.SelecionarRegistroPorPlacaAsync(placaPadronizada, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculoExistente);

        RegistroEntrada registroEntrada1 = new() { Id = registroIdPadrao };
        RegistroEntrada registroEntrada2 = new() { Id = Guid.NewGuid() };
        List<RegistroEntrada> registrosEntrada = new() { registroEntrada1, registroEntrada2 };

        repositorioRegistroEntradaMock
            .Setup(r => r.SelecionarRegistrosDoVeiculoAsync(veiculoExistente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registrosEntrada);

        ImmutableList<SelecionarRegistrosEntradaDto> registrosDto = registrosEntrada.ConvertAll(r =>
            new SelecionarRegistrosEntradaDto(r.Id, r.DataEntradaEmUtc, r.Observacoes, r.HospedeId,
                nomeCompletoPadrao, veiculoIdPadrao, placaPadrao, numeroTicketPadrao
            )).ToImmutableList();

        SelecionarRegistrosDoVeiculoResult resultadoMapeado = new(registrosDto);

        mapperMock
            .Setup(m => m.Map<SelecionarRegistrosDoVeiculoResult>(It.IsAny<List<RegistroEntrada>>()))
            .Returns(resultadoMapeado);

        // Act
        Result<SelecionarRegistrosDoVeiculoResult> resultado = await handler.Handle(query, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        repositorioVeiculoMock.Verify(r =>
            r.SelecionarRegistroPorPlacaAsync(placaPadronizada, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);
        repositorioRegistroEntradaMock.Verify(r =>
            r.SelecionarRegistrosDoVeiculoAsync(veiculoExistente.Id, It.IsAny<CancellationToken>()), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        CollectionAssert.AreEquivalent(resultadoMapeado.RegistrosEntrada, resultado.Value.RegistrosEntrada);
    }

    [TestMethod]
    public async Task Query_Deve_Registros_Entradas_Do_Veiculo_Com_Sucesso_Usando_Id()
    {
        // Arrange
        const int quantidade = 0;
        SelecionarRegistrosDoVeiculoQuery query = new(quantidade, veiculoIdPadrao, null);

        validatorMock
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Estacionamento estacionamento = new() { Id = estacionamentoIdPadrao, Nome = nomeEstacionamentoPadrao };

        Veiculo veiculoExistente = new() { Id = veiculoIdPadrao };
        repositorioVeiculoMock
            .Setup(r => r.SelecionarRegistroPorIdAsync(veiculoIdPadrao))
            .ReturnsAsync(veiculoExistente);

        RegistroEntrada registroEntrada1 = new() { Id = registroIdPadrao };
        RegistroEntrada registroEntrada2 = new() { Id = Guid.NewGuid() };
        List<RegistroEntrada> registrosEntrada = new() { registroEntrada1, registroEntrada2 };

        repositorioRegistroEntradaMock
            .Setup(r => r.SelecionarRegistrosDoVeiculoAsync(veiculoExistente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(registrosEntrada);

        ImmutableList<SelecionarRegistrosEntradaDto> registrosDto = registrosEntrada.ConvertAll(r =>
            new SelecionarRegistrosEntradaDto(r.Id, r.DataEntradaEmUtc, r.Observacoes, r.HospedeId,
                nomeCompletoPadrao, veiculoIdPadrao, placaPadrao, numeroTicketPadrao
            )).ToImmutableList();

        SelecionarRegistrosDoVeiculoResult resultadoMapeado = new(registrosDto);

        mapperMock
            .Setup(m => m.Map<SelecionarRegistrosDoVeiculoResult>(It.IsAny<List<RegistroEntrada>>()))
            .Returns(resultadoMapeado);

        // Act
        Result<SelecionarRegistrosDoVeiculoResult> resultado = await handler.Handle(query, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        repositorioVeiculoMock.Verify(r => r.SelecionarRegistroPorIdAsync(veiculoIdPadrao), Times.Once);
        repositorioRegistroEntradaMock.Verify(r =>
        r.SelecionarRegistrosDoVeiculoAsync(veiculoExistente.Id, It.IsAny<CancellationToken>()), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        CollectionAssert.AreEquivalent(resultadoMapeado.RegistrosEntrada, resultado.Value.RegistrosEntrada);
    }
}
