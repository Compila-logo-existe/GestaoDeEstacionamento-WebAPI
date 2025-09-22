using FizzWare.NBuilder;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloEstacionamento;

[TestClass]
[TestCategory("Testes de Unidade de Vaga (Domínio)")]
public class VagaTestes
{
    private Vaga? vaga;

    [TestMethod]
    public void Deve_Criar_Vaga_Com_Sucesso()
    {
        // Act
        vaga = new Vaga
        {
            Numero = 12,
            Zona = ZonaEstacionamento.B
        };

        // Assert
        Assert.AreEqual(12, vaga.Numero);
        Assert.AreEqual(ZonaEstacionamento.B, vaga.Zona);
        Assert.AreEqual(StatusVaga.Livre, vaga.Status);
        Assert.IsFalse(vaga.EstaOcupada);
        Assert.IsNull(vaga.Veiculo);
        Assert.IsNull(vaga.VeiculoId);
    }

    [TestMethod]
    public void Deve_Ocupar_Vaga_Com_Sucesso()
    {
        // Arrange
        vaga = new Vaga { Numero = 1, Zona = ZonaEstacionamento.A };
        Veiculo veiculo = Builder<Veiculo>.CreateNew().Build();

        // Act
        vaga.Ocupar(veiculo);

        // Assert
        Assert.IsTrue(vaga.EstaOcupada);
        Assert.AreEqual(StatusVaga.Ocupada, vaga.Status);
        Assert.AreEqual(veiculo, vaga.Veiculo);
        Assert.AreEqual(veiculo.Id, vaga.VeiculoId);
    }

    [TestMethod]
    public void Nao_Deve_Ocupar_Vaga_Ja_Ocupada()
    {
        // Arrange
        vaga = new Vaga { Numero = 1, Zona = ZonaEstacionamento.A };
        Veiculo veiculoA = Builder<Veiculo>.CreateNew().Build();
        Veiculo veiculoB = Builder<Veiculo>.CreateNew().Build();

        vaga.Ocupar(veiculoA);

        // Act
        vaga.Ocupar(veiculoB);

        // Assert
        Assert.IsTrue(vaga.EstaOcupada);
        Assert.AreEqual(StatusVaga.Ocupada, vaga.Status);
        Assert.AreEqual(veiculoA, vaga.Veiculo);
        Assert.AreEqual(veiculoA.Id, vaga.VeiculoId);
    }

    [TestMethod]
    public void Deve_Liberar_Vaga_Com_Sucesso()
    {
        // Arrange
        vaga = new Vaga { Numero = 2, Zona = ZonaEstacionamento.C };
        Veiculo veiculo = Builder<Veiculo>.CreateNew().Build();
        vaga.Ocupar(veiculo);

        // Act
        vaga.Liberar();

        // Assert
        Assert.IsFalse(vaga.EstaOcupada);
        Assert.AreEqual(StatusVaga.Livre, vaga.Status);
        Assert.IsNull(vaga.Veiculo);
        Assert.IsNull(vaga.VeiculoId);
    }

    [TestMethod]
    public void Nao_Deve_Liberar_Quando_Ja_Estiver_Livre()
    {
        // Arrange
        vaga = new Vaga { Numero = 3, Zona = ZonaEstacionamento.D };

        // Act
        vaga.Liberar();

        // Assert
        Assert.IsFalse(vaga.EstaOcupada);
        Assert.AreEqual(StatusVaga.Livre, vaga.Status);
        Assert.IsNull(vaga.Veiculo);
        Assert.IsNull(vaga.VeiculoId);
    }

    [TestMethod]
    public void Deve_Atualizar_Registro_Com_Sucesso()
    {
        // Arrange
        vaga = new Vaga { Numero = 5, Zona = ZonaEstacionamento.A };
        Vaga vagaEditada = new() { Numero = 9, Zona = ZonaEstacionamento.C };

        // Act
        vaga.AtualizarRegistro(vagaEditada);

        // Assert
        Assert.AreEqual(9, vaga.Numero);
        Assert.AreEqual(ZonaEstacionamento.C, vaga.Zona);
    }
}
