using AutoMapper;
using FizzWare.NBuilder;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Handlers;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloRecepcaoCheckin;

[TestClass]
[TestCategory("Testes de Unidade de SelecionarRegistrosEntradaQueryHandler")]
public class SelecionarRegistrosEntradaQueryHandlerTestes
{
    private SelecionarRegistrosEntradaQueryHandler handler = null!;

    private const string nomeEstacionamentoPadrao = "Estacionamento Central";
    private const int numeroTicketPadrao = 22;
    private const string placaPadrao = "ABC1D23";
    private const string nomeCompletoPadrao = "Fulano da Silva";

    private readonly Guid tenantIdPadrao = Guid.NewGuid();
    private readonly Guid usuarioIdPadrao = Guid.NewGuid();
    private readonly Guid estacionamentoIdPadrao = Guid.NewGuid();
    private readonly Guid veiculoIdPadrao = Guid.NewGuid();

    private Mock<IMapper> mapperMock = null!;
    private Mock<IRepositorioRegistroEntrada> repositorioRegistroEntradaMock = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<IDistributedCache> cacheMock = null!;
    private Mock<ILogger<SelecionarRegistrosEntradaQueryHandler>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        mapperMock = new Mock<IMapper>();
        repositorioRegistroEntradaMock = new Mock<IRepositorioRegistroEntrada>();
        tenantProviderMock = new Mock<ITenantProvider>();
        cacheMock = new Mock<IDistributedCache>();
        loggerMock = new Mock<ILogger<SelecionarRegistrosEntradaQueryHandler>>();

        tenantProviderMock.SetupGet(p => p.TenantId)
            .Returns(tenantIdPadrao);
        tenantProviderMock.SetupGet(p => p.UsuarioId)
            .Returns(usuarioIdPadrao);

        handler = new SelecionarRegistrosEntradaQueryHandler(
            mapperMock.Object,
            repositorioRegistroEntradaMock.Object,
            tenantProviderMock.Object,
            cacheMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Query_Deve_Selecionar_Todos_Registros_Entradas_Com_Sucesso()
    {
        // Arrange
        const int quantidade = 0;
        SelecionarRegistrosEntradaQuery query = new(quantidade);

        Estacionamento estacionamento = new() { Id = estacionamentoIdPadrao, Nome = nomeEstacionamentoPadrao };

        List<RegistroEntrada> registrosEntrada = Builder<RegistroEntrada>.CreateListOfSize(3).Build().ToList();

        repositorioRegistroEntradaMock
            .Setup(r => r.SelecionarRegistrosAsync(quantidade))
            .ReturnsAsync(registrosEntrada);

        ImmutableList<SelecionarRegistrosEntradaDto> registrosDto = registrosEntrada.ConvertAll(r =>
        new SelecionarRegistrosEntradaDto(
            r.Id,
            r.DataEntradaEmUtc,
            r.Observacoes,
            r.HospedeId,
            nomeCompletoPadrao,
            veiculoIdPadrao,
            placaPadrao,
            numeroTicketPadrao
        )).ToImmutableList();

        SelecionarRegistrosEntradaResult resultadoMapeado = new(registrosDto);
        mapperMock
            .Setup(m => m.Map<SelecionarRegistrosEntradaResult>(It.IsAny<List<RegistroEntrada>>()))
            .Returns(resultadoMapeado);

        string chaveCacheEsperada = $"recepcao:t={tenantIdPadrao}:q={quantidade}";
        // Setup para simular que o cache não possui o resultado

        // Act
        Result<SelecionarRegistrosEntradaResult> resultado = await handler.Handle(query, CancellationToken.None);

        // Assert
        repositorioRegistroEntradaMock.Verify(r => r.SelecionarRegistrosAsync(quantidade), Times.Once);
        // Verifica se o cache foi consultado

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        CollectionAssert.AreEquivalent(resultado.Value.RegistrosEntrada, resultadoMapeado.RegistrosEntrada);
    }

    [TestMethod]
    public async Task Query_Deve_Selecionar_Dois_Registros_Entradas_Com_Sucesso()
    {
        // Arrange
        const int quantidade = 2;
        SelecionarRegistrosEntradaQuery query = new(quantidade);

        Estacionamento estacionamento = new() { Id = estacionamentoIdPadrao, Nome = nomeEstacionamentoPadrao };

        List<RegistroEntrada> registrosEntrada = Builder<RegistroEntrada>.CreateListOfSize(3).Build().ToList();

        repositorioRegistroEntradaMock
            .Setup(r => r.SelecionarRegistrosAsync(quantidade))
            .ReturnsAsync(registrosEntrada.Take(quantidade).ToList());

        ImmutableList<SelecionarRegistrosEntradaDto> registrosDto = registrosEntrada.ConvertAll(r =>
        new SelecionarRegistrosEntradaDto(
            r.Id,
            r.DataEntradaEmUtc,
            r.Observacoes,
            r.HospedeId,
            nomeCompletoPadrao,
            veiculoIdPadrao,
            placaPadrao,
            numeroTicketPadrao
        )).Take(quantidade).ToImmutableList();

        SelecionarRegistrosEntradaResult resultadoMapeado = new(registrosDto);
        mapperMock
            .Setup(m => m.Map<SelecionarRegistrosEntradaResult>(It.IsAny<List<RegistroEntrada>>()))
            .Returns(resultadoMapeado);

        string chaveCacheEsperada = $"recepcao:t={tenantIdPadrao}:q={quantidade}";
        // Setup para simular que o cache não possui o resultado

        // Act
        Result<SelecionarRegistrosEntradaResult> resultado = await handler.Handle(query, CancellationToken.None);

        // Assert
        repositorioRegistroEntradaMock.Verify(r => r.SelecionarRegistrosAsync(quantidade), Times.Once);
        // Verifica se o cache foi consultado

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.IsTrue(resultado.Value.RegistrosEntrada.Count == 2);
        CollectionAssert.AreEquivalent(resultado.Value.RegistrosEntrada, resultadoMapeado.RegistrosEntrada);
    }
}
