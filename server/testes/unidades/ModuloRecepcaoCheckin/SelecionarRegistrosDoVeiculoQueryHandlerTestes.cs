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
using System.Security.Cryptography;
using System.Text;

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

        string placaPadronizadaParaCache = Padronizador.PadronizarPlaca(query.Placa);
        string placaHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(placaPadronizadaParaCache)));

        Estacionamento estacionamento = new() { Id = estacionamentoIdPadrao, Nome = nomeEstacionamentoPadrao };

        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SelecionarRegistrosDoVeiculoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Veiculo veiculoExistente = new() { Id = veiculoIdPadrao };
        repositorioVeiculoMock
            .Setup(r => r.SelecionarRegistroPorPlacaAsync(placaPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()))
            .ReturnsAsync(veiculoExistente);

        RegistroEntrada registroEntradaMapeado = new() { Id = registroIdPadrao };
        RegistroEntrada registroEntradaMapeado2 = new() { Id = registroIdPadrao };
        List<RegistroEntrada> registrosEntrada = new() { registroEntradaMapeado, registroEntradaMapeado2 };

        repositorioRegistroEntradaMock
            .Setup(r => r.SelecionarRegistrosDoVeiculoAsync(quantidade, veiculoExistente.Id, It.IsAny<CancellationToken>()))
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

        SelecionarRegistrosDoVeiculoResult resultadoMapeado = new(registrosDto);
        mapperMock
            .Setup(m => m.Map<SelecionarRegistrosDoVeiculoResult>(It.IsAny<List<RegistroEntrada>>()))
            .Returns(resultadoMapeado);

        string chaveCacheEsperada = $"recepcao:t={tenantIdPadrao}:q={quantidade}:v={placaHash}";
        // Setup para simular que o cache não possui o resultado

        // Act
        Result<SelecionarRegistrosDoVeiculoResult> resultado = await handler.Handle(query, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        repositorioVeiculoMock.Verify(r => r.SelecionarRegistroPorPlacaAsync(placaPadrao, tenantIdPadrao,
            It.IsAny<CancellationToken>()), Times.Once
        );
        repositorioRegistroEntradaMock.Verify(r => r.SelecionarRegistrosDoVeiculoAsync(quantidade, veiculoExistente.Id,
            It.IsAny<CancellationToken>()), Times.Once
        );
        // Verifica se o cache foi consultado

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        CollectionAssert.AreEquivalent(resultado.Value.RegistrosEntrada, resultadoMapeado.RegistrosEntrada);
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

        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<SelecionarRegistrosDoVeiculoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Veiculo veiculoExistente = new() { Id = veiculoIdPadrao };
        repositorioVeiculoMock
            .Setup(r => r.SelecionarRegistroPorIdAsync(veiculoIdPadrao))
            .ReturnsAsync(veiculoExistente);

        RegistroEntrada registroEntradaMapeado = new() { Id = registroIdPadrao };
        RegistroEntrada registroEntradaMapeado2 = new() { Id = registroIdPadrao };
        List<RegistroEntrada> registrosEntrada = new() { registroEntradaMapeado, registroEntradaMapeado2 };

        repositorioRegistroEntradaMock
            .Setup(r => r.SelecionarRegistrosDoVeiculoAsync(quantidade, veiculoExistente.Id, It.IsAny<CancellationToken>()))
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

        SelecionarRegistrosDoVeiculoResult resultadoMapeado = new(registrosDto);
        mapperMock
            .Setup(m => m.Map<SelecionarRegistrosDoVeiculoResult>(It.IsAny<List<RegistroEntrada>>()))
            .Returns(resultadoMapeado);

        string chaveCacheEsperada = $"recepcao:t={tenantIdPadrao}:q={quantidade}:v={veiculoIdPadrao}";
        // Setup para simular que o cache não possui o resultado

        // Act
        Result<SelecionarRegistrosDoVeiculoResult> resultado = await handler.Handle(query, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        repositorioVeiculoMock.Verify(r => r.SelecionarRegistroPorIdAsync(veiculoIdPadrao), Times.Once);
        repositorioRegistroEntradaMock.Verify(r => r.SelecionarRegistrosDoVeiculoAsync(quantidade, veiculoExistente.Id,
            It.IsAny<CancellationToken>()), Times.Once
        );
        // Verifica se o cache foi consultado

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        CollectionAssert.AreEquivalent(resultado.Value.RegistrosEntrada, resultadoMapeado.RegistrosEntrada);
    }
}
