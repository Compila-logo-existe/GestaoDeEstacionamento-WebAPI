using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Handlers;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloEstacionamento;

[TestClass]
[TestCategory("Testes de Unidade de ObterStatusVagasQueryHandler")]
public class ObterStatusVagasQueryHandlerTestes
{
    private ObterStatusVagasQueryHandler handler = null!;

    private const string nomeEstacionamentoPadrao = "Estacionamento Central";
    private const string zonaPadrao = "A";
    private const int numeroVagaPadrao = 10;

    private readonly Guid tenantIdPadrao = Guid.NewGuid();
    private readonly Guid usuarioIdPadrao = Guid.NewGuid();
    private readonly Guid estacionamentoIdPadrao = Guid.NewGuid();
    private readonly Guid vagaIdPadrao = Guid.NewGuid();

    private Mock<IValidator<ObterStatusVagasQuery>> validatorMock = null!;
    private Mock<IMapper> mapperMock = null!;
    private Mock<IRepositorioEstacionamento> repositorioEstacionamentoMock = null!;
    private Mock<IRepositorioVaga> repositorioVagaMock = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<IDistributedCache> cacheMock = null!;
    private Mock<ILogger<ObterStatusVagasQueryHandler>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        validatorMock = new Mock<IValidator<ObterStatusVagasQuery>>();
        mapperMock = new Mock<IMapper>();
        repositorioEstacionamentoMock = new Mock<IRepositorioEstacionamento>();
        repositorioVagaMock = new Mock<IRepositorioVaga>();
        tenantProviderMock = new Mock<ITenantProvider>();
        cacheMock = new Mock<IDistributedCache>();
        loggerMock = new Mock<ILogger<ObterStatusVagasQueryHandler>>();

        tenantProviderMock.SetupGet(p => p.TenantId).Returns(tenantIdPadrao);
        tenantProviderMock.SetupGet(p => p.UsuarioId).Returns(usuarioIdPadrao);

        handler = new ObterStatusVagasQueryHandler(
            validatorMock.Object,
            mapperMock.Object,
            repositorioEstacionamentoMock.Object,
            repositorioVagaMock.Object,
            tenantProviderMock.Object,
            cacheMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Query_Deve_Obter_Status_Das_Vagas_Com_Sucesso_Usando_Nome_Estacionamento()
    {
        // Arrange
        const int quantidade = 0;
        ObterStatusVagasQuery query = new(quantidade, null,
            nomeEstacionamentoPadrao, null, null);

        validatorMock
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Estacionamento estacionamento = new() { Id = estacionamentoIdPadrao, Nome = nomeEstacionamentoPadrao };
        Vaga vaga1 = new() { Id = vagaIdPadrao, Estacionamento = estacionamento, Numero = numeroVagaPadrao, Zona = ZonaEstacionamento.A };
        Vaga vaga2 = new() { Id = Guid.NewGuid(), Estacionamento = estacionamento, Numero = numeroVagaPadrao + 1, Zona = ZonaEstacionamento.A };
        List<Vaga> vagasExistentes = new() { vaga1, vaga2 };

        repositorioEstacionamentoMock
            .Setup(r => r.SelecionarRegistroPorNome(nomeEstacionamentoPadrao, tenantIdPadrao,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(estacionamento);

        repositorioVagaMock
            .Setup(r => r.SelecionarRegistrosDoEstacionamentoAsync(
                quantidade,
                estacionamentoIdPadrao,
                null!,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vagasExistentes);

        string chaveCacheEsperada = $"estacionamento:t={tenantIdPadrao}:q={quantidade}:e={nomeEstacionamentoPadrao}:z={zonaPadrao}";
        // Setup para simular que o cache não possui o resultado

        ImmutableList<ObterStatusVagasDto> vagasDto = vagasExistentes.ConvertAll(vaga =>
        new ObterStatusVagasDto(
            vaga.Id,
            vaga.Numero,
            vaga.Zona,
            vaga.Status,
            vaga.Veiculo?.Placa
        )).ToImmutableList();

        ObterStatusVagasResult resultadoMapeado = new(vagasDto);
        mapperMock
            .Setup(m => m.Map<ObterStatusVagasResult>(It.IsAny<List<Vaga>>()))
            .Returns(resultadoMapeado);

        // Act
        Result<ObterStatusVagasResult> resultado = await handler.Handle(query, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        repositorioEstacionamentoMock.Verify(r =>
            r.SelecionarRegistroPorNome(nomeEstacionamentoPadrao, tenantIdPadrao, It.IsAny<CancellationToken>()),
            Times.Once
        );

        repositorioVagaMock.Verify(r =>
            r.SelecionarRegistrosDoEstacionamentoAsync(
                quantidade,
                estacionamentoIdPadrao,
                null,
                It.IsAny<CancellationToken>()
            ), Times.Once
        );
        // Verifica se o cache foi consultado

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        CollectionAssert.AreEquivalent(resultado.Value.Vagas, resultadoMapeado.Vagas);
    }

    [TestMethod]
    public async Task Query_Deve_Obter_Status_Das_Vagas_Com_Sucesso_Usando_Id_Estacionamento()
    {
        // Arrange
        const int quantidade = 0;
        ObterStatusVagasQuery query = new(quantidade, estacionamentoIdPadrao,
            null, null, null);

        validatorMock
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Estacionamento estacionamento = new() { Id = estacionamentoIdPadrao, Nome = nomeEstacionamentoPadrao };
        Vaga vaga1 = new() { Id = vagaIdPadrao, Estacionamento = estacionamento, Numero = numeroVagaPadrao, Zona = ZonaEstacionamento.A };
        Vaga vaga2 = new() { Id = Guid.NewGuid(), Estacionamento = estacionamento, Numero = numeroVagaPadrao + 1, Zona = ZonaEstacionamento.A };
        List<Vaga> vagasExistentes = new() { vaga1, vaga2 };

        repositorioEstacionamentoMock
            .Setup(r => r.SelecionarRegistroPorIdAsync(estacionamentoIdPadrao))
            .ReturnsAsync(estacionamento);

        repositorioVagaMock
            .Setup(r => r.SelecionarRegistrosDoEstacionamentoAsync(
                quantidade,
                estacionamentoIdPadrao,
                null!,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(vagasExistentes);

        string chaveCacheEsperada = $"estacionamento:t={tenantIdPadrao}:q={quantidade}:e={nomeEstacionamentoPadrao}:z={zonaPadrao}";
        // Setup para simular que o cache não possui o resultado

        ImmutableList<ObterStatusVagasDto> vagasDto = vagasExistentes.ConvertAll(vaga =>
        new ObterStatusVagasDto(
            vaga.Id,
            vaga.Numero,
            vaga.Zona,
            vaga.Status,
            vaga.Veiculo?.Placa
        )).ToImmutableList();

        ObterStatusVagasResult resultadoMapeado = new(vagasDto);
        mapperMock
            .Setup(m => m.Map<ObterStatusVagasResult>(It.IsAny<List<Vaga>>()))
            .Returns(resultadoMapeado);

        // Act
        Result<ObterStatusVagasResult> resultado = await handler.Handle(query, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        repositorioEstacionamentoMock.Verify(r =>
            r.SelecionarRegistroPorIdAsync(estacionamentoIdPadrao), Times.Once);

        repositorioVagaMock.Verify(r =>
            r.SelecionarRegistrosDoEstacionamentoAsync(
                quantidade,
                estacionamentoIdPadrao,
                null,
                It.IsAny<CancellationToken>()
            ), Times.Once
        );
        // Verifica se o cache foi consultado

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        CollectionAssert.AreEquivalent(resultado.Value.Vagas, resultadoMapeado.Vagas);
    }
}