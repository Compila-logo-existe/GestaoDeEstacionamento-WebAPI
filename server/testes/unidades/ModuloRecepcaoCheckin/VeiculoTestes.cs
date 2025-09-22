using FizzWare.NBuilder;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloRecepcaoCheckin;

[TestClass]
[TestCategory("Testes de Unidade de Veiculo (Domínio)")]
public class VeiculoTestes
{
    private Veiculo? veiculo;
    private static readonly Hospede hospede = Builder<Hospede>.CreateNew().Build();
    private const string placa = "ABC-1234";
    private const string modelo = "Aquele";
    private const string cor = "Rosa";

    [TestMethod]
    public void Deve_Criar_Veiculo_Com_Sucesso()
    {
        // Arrange 
        List<string> observacoes = new() { "Observação 1", "Observação 2" };

        // Act
        Veiculo veiculo = Builder<Veiculo>
            .CreateNew()
            .WithFactory(() => new(placa, modelo, cor, hospede, observacoes))
            .Build();

        // Assert
        Assert.AreEqual(veiculo.Hospede, hospede);
        Assert.AreEqual(veiculo.Placa, placa);
        Assert.AreEqual(veiculo.Modelo, modelo);
        Assert.AreEqual(veiculo.Cor, cor);
        CollectionAssert.AreEquivalent(veiculo.Observacoes, observacoes);
    }

    [TestMethod]
    public void Deve_Aderir_Hospede_Com_Sucesso()
    {
        // Arrange 
        veiculo = new(placa, modelo, cor, null!, new List<string>());

        // Act
        veiculo.AderirHospede(hospede);

        // Assert
        Assert.AreEqual(veiculo.Hospede, hospede);
    }

    [TestMethod]
    public void Deve_Se_Vincular_Com_o_Hospede_Ao_Aderir_Hospede_Com_Sucesso()
    {
        // Arrange 
        Veiculo veiculoT = Builder<Veiculo>.CreateNew().With(v => v.Id == Guid.NewGuid()).Build();
        hospede.AderirVeiculo(veiculoT);

        veiculo = new(placa, modelo, cor, null!, new List<string>());

        // Act
        veiculo.AderirHospede(hospede);

        // Assert
        Assert.AreEqual(veiculo.Hospede, hospede);
    }

    [TestMethod]
    public void Deve_Atualizar_Veiculo_Com_Sucesso()
    {
        // Arrange
        veiculo = new(placa, modelo, cor, hospede, new List<string>());
        Veiculo veiculoEditado = Builder<Veiculo>
            .CreateNew()
            .With(r => r.Observacoes = new List<string> { "Observação 1", "Observação 2" })
            .Build();

        // Act
        veiculo.AtualizarRegistro(veiculoEditado);

        // Assert
        Assert.AreEqual(veiculo.Hospede, veiculoEditado.Hospede);
        Assert.AreEqual(veiculo.Placa, veiculoEditado.Placa);
        Assert.AreEqual(veiculo.Modelo, veiculoEditado.Modelo);
        Assert.AreEqual(veiculo.Cor, veiculoEditado.Cor);
        CollectionAssert.AreEquivalent(veiculo.Observacoes, veiculoEditado.Observacoes);
    }

    [TestMethod]
    public void Nao_Deve_Atualizar_Veiculo_Com_Sucesso()
    {
        // Arrange
        veiculo = new(placa, modelo, cor, hospede, new List<string>());

        // Act
        veiculo.AtualizarRegistro(null!);

        // Assert
        Assert.IsTrue(veiculo.Observacoes.Count <= 0);
    }
}
