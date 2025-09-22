using AutoMapper;
using FizzWare.NBuilder;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Commands;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Handlers;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloFaturamento;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloFaturamento;

[TestClass]
[TestCategory("Testes de Unidade de GerarRelatorioFinanceiroQueryHandler")]
public class GerarRelatorioFinanceiroQueryHandlerTestes
{
    private GerarRelatorioFinanceiroQueryHandler handler = null!;

    private DateTime dataInicialPadrao = DateTime.UtcNow.AddDays(-14);
    private DateTime dataFinalPadrao = DateTime.UtcNow.AddDays(1);
    private const decimal valorDiariaPadrao = 50.0m;

    private readonly Guid tenantIdPadrao = Guid.NewGuid();
    private readonly Guid usuarioIdPadrao = Guid.NewGuid();

    private Mock<IValidator<GerarRelatorioFinanceiroQuery>> validatorMock = null!;
    private Mock<IMapper> mapperMock = null!;
    private Mock<IRepositorioFaturamento> repositorioFaturamentoMock = null!;
    private Mock<ITenantProvider> tenantProviderMock = null!;
    private Mock<ILogger<GerarRelatorioFinanceiroQueryHandler>> loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        validatorMock = new Mock<IValidator<GerarRelatorioFinanceiroQuery>>();
        mapperMock = new Mock<IMapper>();
        repositorioFaturamentoMock = new Mock<IRepositorioFaturamento>();
        tenantProviderMock = new Mock<ITenantProvider>();
        loggerMock = new Mock<ILogger<GerarRelatorioFinanceiroQueryHandler>>();

        tenantProviderMock.SetupGet(p => p.TenantId)
            .Returns(tenantIdPadrao);
        tenantProviderMock.SetupGet(p => p.UsuarioId)
            .Returns(usuarioIdPadrao);

        handler = new GerarRelatorioFinanceiroQueryHandler(
            validatorMock.Object,
            mapperMock.Object,
            repositorioFaturamentoMock.Object,
            tenantProviderMock.Object,
            loggerMock.Object
        );
    }

    [TestMethod]
    public async Task Query_Deve_Gerar_Relatorio_Financeiro_Com_Sucesso()
    {
        // Arrange
        GerarRelatorioFinanceiroQuery query = new(dataInicialPadrao, dataFinalPadrao);

        validatorMock
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        List<Faturamento> faturamentos = Builder<Faturamento>.CreateListOfSize(3)
            .All().With(f => f.Id = Guid.NewGuid()).Build().ToList();

        faturamentos[0].DataEntradaEmUtc = DateTime.UtcNow.AddDays(-11);
        faturamentos[0].DefinirValorDiaria(valorDiariaPadrao);
        faturamentos[1].DataEntradaEmUtc = DateTime.UtcNow.AddDays(-7);
        faturamentos[1].DefinirValorDiaria(valorDiariaPadrao);

        repositorioFaturamentoMock
                .Setup(r => r.SelecionarPorPeriodoAsync(query.DataInicial, query.DataFinal, tenantIdPadrao, It.IsAny<CancellationToken>()))
                .ReturnsAsync(faturamentos);

        decimal valorTotalPeriodo = faturamentos.Sum(i => i.ValorTotal);

        ImmutableList<FaturamentoDto> faturamentosDto = faturamentos.ConvertAll(f =>
        new FaturamentoDto(
            f.Id,
            f.RegistroEntradaId,
            f.RegistroSaida != null ? f.RegistroSaida.Id : Guid.Empty,
            f.DataEntradaEmUtc,
            f.RegistroSaida != null ? f.RegistroSaida.DataSaidaEmUtc : null,
            f.ValorDaDiaria,
            f.NumeroDeDiarias,
            f.ValorTotal
        )).ToImmutableList();

        GerarRelatorioFinanceiroResult resultadoMapeado = new(dataInicialPadrao, dataFinalPadrao,
            faturamentos.Count, valorTotalPeriodo, faturamentosDto);
        mapperMock
            .Setup(m => m.Map<GerarRelatorioFinanceiroResult>(It.IsAny<(GerarRelatorioFinanceiroQuery, List<Faturamento>, decimal)>()))
            .Returns(resultadoMapeado);

        // Act
        Result<GerarRelatorioFinanceiroResult> resultado = await handler.Handle(query, CancellationToken.None);

        // Assert
        validatorMock.Verify(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()), Times.Once);

        repositorioFaturamentoMock.Verify(r =>
            r.SelecionarPorPeriodoAsync(query.DataInicial, query.DataFinal, tenantIdPadrao, It.IsAny<CancellationToken>()), Times.Once);

        mapperMock.Verify(m => m.Map<GerarRelatorioFinanceiroResult>(
                It.IsAny<(GerarRelatorioFinanceiroQuery, List<Faturamento>, decimal)>()), Times.Once);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.IsSuccess);
        Assert.AreSame(resultadoMapeado, resultado.Value);
    }

}
