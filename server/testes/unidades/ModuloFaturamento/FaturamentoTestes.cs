using FizzWare.NBuilder;
using GestaoDeEstacionamento.Core.Dominio.ModuloFaturamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloFaturamento;

[TestClass]
[TestCategory("Testes de Unidade de Faturamento (Domínio)")]
public class FaturamentoTestes
{
    private Faturamento? faturamento;
    private readonly RegistroSaida registroSaida = Builder<RegistroSaida>.CreateNew()
        .WithFactory(() => new(hospede, veiculo, new List<string>())).Build();
    private readonly RegistroEntrada registroEntrada = Builder<RegistroEntrada>.CreateNew()
        .WithFactory(() => new(hospede, veiculo, new List<string>())).Build();
    private static readonly Hospede hospede = Builder<Hospede>.CreateNew().Build();
    private static readonly Veiculo veiculo = Builder<Veiculo>.CreateNew().Build();

    [TestMethod]
    public void Deve_Aderir_Registro_Entrada_Com_Sucesso()
    {
        // Arrange 
        registroEntrada.GerarNovoTicket();
        registroEntrada.GerarNovoFaturamento(5);
        faturamento = registroEntrada.Faturamento;

        // Act
        faturamento.AderirRegistroEntrada(registroEntrada);

        // Assert
        Assert.AreEqual(registroEntrada, faturamento.RegistroEntrada);
        Assert.AreEqual(registroEntrada.Id, faturamento.RegistroEntradaId);
    }

    [TestMethod]
    public void Não_Deve_Aderir_Registro_Entrada()
    {
        // Arrange 
        faturamento = new();

        // Act
        faturamento.AderirRegistroEntrada(null!);

        // Assert
        Assert.AreNotEqual(registroEntrada, faturamento.RegistroEntrada);
        Assert.AreNotEqual(registroEntrada.Id, faturamento.RegistroEntradaId);
    }

    [TestMethod]
    public void Deve_Definir_Valor_Diaria_Com_Sucesso()
    {
        // Arrange 
        faturamento = new Faturamento
        {
            DataEntradaEmUtc = DateTime.UtcNow.AddMinutes(1)
        };

        // Act
        faturamento.DefinirValorDiaria(10m);

        // Assert
        Assert.AreEqual(10m, faturamento.ValorDaDiaria);
        Assert.AreEqual(1, faturamento.NumeroDeDiarias);
        Assert.AreEqual(10m, faturamento.ValorTotal);
    }

    [TestMethod]
    public void Deve_Recalcular_Totais_Com_Dias_Exatos()
    {
        // Arrange
        DateTime inicio = DateTime.UtcNow.Date.AddDays(-2);
        DateTime fim = inicio.AddDays(2);

        faturamento = new Faturamento
        {
            ValorDaDiaria = 10m,
            DataEntradaEmUtc = inicio,
            RegistroSaida = Builder<RegistroSaida>
                .CreateNew()
                .WithFactory(() => new(hospede, veiculo, new List<string>()))
                .Build()
        };
        faturamento.RegistroSaida!.DataSaidaEmUtc = fim;

        // Act
        faturamento.RecalcularTotais();

        // Assert
        Assert.AreEqual(2, faturamento.NumeroDeDiarias);
        Assert.AreEqual(20m, faturamento.ValorTotal);
    }

    [TestMethod]
    public void Deve_Recalcular_Totais_Com_Resto_De_Dia()
    {
        // Arrange 
        DateTime inicio = DateTime.UtcNow.Date.AddDays(-2);
        DateTime fim = inicio.AddDays(2).AddMinutes(1);

        faturamento = new Faturamento
        {
            ValorDaDiaria = 7m,
            DataEntradaEmUtc = inicio,
            RegistroSaida = Builder<RegistroSaida>
                .CreateNew()
                .WithFactory(() => new(hospede, veiculo, new List<string>()))
                .Build()
        };
        faturamento.RegistroSaida!.DataSaidaEmUtc = fim;

        // Act
        faturamento.RecalcularTotais();

        // Assert
        Assert.AreEqual(3, faturamento.NumeroDeDiarias);
        Assert.AreEqual(21m, faturamento.ValorTotal);
    }

    [TestMethod]
    public void Deve_Atualizar_Faturamento_Com_Sucesso()
    {
        // Arrange
        registroEntrada.GerarNovoTicket();
        registroEntrada.GerarNovoFaturamento(5);
        faturamento = registroEntrada.Faturamento;

        Faturamento faturamentoEditado = Builder<Faturamento>.CreateNew().Build();

        // Act
        faturamento.AtualizarRegistro(faturamentoEditado);

        // Assert
        Assert.AreEqual(faturamentoEditado.ValorDaDiaria, faturamento.ValorDaDiaria);
        Assert.AreEqual(faturamentoEditado.NumeroDeDiarias, faturamento.NumeroDeDiarias);
        Assert.AreEqual(faturamentoEditado.ValorTotal, faturamento.ValorTotal);
    }

    [TestMethod]
    public void Nao_Deve_Atualizar_Faturamento()
    {
        // Arrange
        registroEntrada.GerarNovoTicket();
        registroEntrada.GerarNovoFaturamento(5);
        faturamento = registroEntrada.Faturamento;

        Faturamento faturamentoEditado = Builder<Faturamento>.CreateNew().Build();

        // Act
        faturamento.AtualizarRegistro(null!);

        // Assert
        Assert.AreNotEqual(faturamentoEditado, faturamento);
    }
}
