using FizzWare.NBuilder;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloHospede;

[TestClass]
[TestCategory("Testes de Unidade de Hospede (Domínio)")]
public class HospedeTestes
{
    private Hospede? hospede;

    [TestMethod]
    public void Deve_Aderir_Registro_Entrada_Com_Sucesso()
    {
        // Act
        hospede = new("Fulano", "12345678909", "(51) 99999-0000");

        // Assert
        Assert.IsNotNull(hospede);
        Assert.AreEqual("Fulano", hospede.NomeCompleto);
        Assert.AreEqual("12345678909", hospede.CPF);
        Assert.AreEqual("(51) 99999-0000", hospede.Telefone);
    }

    [TestMethod]
    public void Deve_Atualizar_Hospede_Com_Sucesso()
    {
        // Arrange
        hospede = new Hospede("Fulano", "12345678909", "(51) 99999-0000");

        Hospede hospedeEditado = Builder<Hospede>.CreateNew()
            .WithFactory(() => new Hospede("Nome Atualizado", "98765432100", "(51) 98888-0000"))
            .Build();

        // Act
        hospede.AtualizarRegistro(hospedeEditado);

        // Assert
        Assert.AreEqual(hospedeEditado.NomeCompleto, hospede.NomeCompleto);
        Assert.AreEqual(hospedeEditado.CPF, hospede.CPF);
        Assert.AreEqual(hospedeEditado.Telefone, hospede.Telefone);
    }

    [TestMethod]
    public void Deve_Vincular_Tenant_Com_Sucesso()
    {
        // Arrange
        hospede = Builder<Hospede>.CreateNew()
            .WithFactory(() => new Hospede("Fulano", "12345678909", "(51) 99999-0000"))
            .Build();

        Guid tenantIdEsperado = Guid.NewGuid();

        // Act
        hospede.VincularTenant(tenantIdEsperado);

        // Assert
        Assert.AreEqual(tenantIdEsperado, hospede.TenantId);
    }

    [TestMethod]
    public void Deve_Possuir_Veiculo_Por_Placa_Com_Sucesso()
    {
        // Arrange
        hospede = Builder<Hospede>.CreateNew()
            .WithFactory(() => new Hospede("Fulano", "12345678909", "(51) 99999-0000"))
            .Build();

        Veiculo veiculo = Builder<Veiculo>.CreateNew()
            .With(v => v.Placa = "ABC1234")
            .Build();

        // Act
        hospede.AderirVeiculo(veiculo);
        bool possuiVeiculo = hospede.PossuiVeiculoPorPlaca("abc1234");

        // Assert
        Assert.IsTrue(possuiVeiculo);
    }

    [TestMethod]
    public void Nao_Deve_Possuir_Veiculo_Quando_Placa_Nao_Existe()
    {
        // Arrange
        hospede = Builder<Hospede>.CreateNew()
            .WithFactory(() => new Hospede("Fulano", "12345678909", "(51) 99999-0000"))
            .Build();

        // Act
        bool possuiVeiculo = hospede.PossuiVeiculoPorPlaca("ZZZ0000");

        // Assert
        Assert.IsFalse(possuiVeiculo);
    }

    [TestMethod]
    public void Deve_Aderir_Veiculo_Com_Sucesso()
    {
        // Arrange
        hospede = Builder<Hospede>.CreateNew()
            .WithFactory(() => new Hospede("Fulano", "12345678909", "(51) 99999-0000"))
            .Build();

        Veiculo veiculo = Builder<Veiculo>.CreateNew()
            .With(v => v.Placa = "ABC1234")
            .Build();

        // Act
        hospede.AderirVeiculo(veiculo);

        // Assert
        Assert.AreEqual(1, hospede.Veiculos.Count);
        Assert.AreEqual(veiculo, hospede.Veiculos[0]);
        Assert.AreEqual(hospede, veiculo.Hospede);
        Assert.AreEqual(hospede.Id, veiculo.HospedeId);
    }

    [TestMethod]
    public void Nao_Deve_Duplicar_Veiculo_Ao_Aderir_Mesma_Placa_Ignorando_Case()
    {
        // Arrange
        hospede = Builder<Hospede>.CreateNew()
            .WithFactory(() => new Hospede("Fulano", "12345678909", "(51) 99999-0000"))
            .Build();

        Veiculo primeiroVeiculo = Builder<Veiculo>.CreateNew()
            .With(v => v.Placa = "ABC1234")
            .Build();

        Veiculo segundoVeiculoMesmaPlaca = Builder<Veiculo>.CreateNew()
            .With(v => v.Placa = "abc1234")
            .Build();

        // Act
        hospede.AderirVeiculo(primeiroVeiculo);
        hospede.AderirVeiculo(segundoVeiculoMesmaPlaca);

        // Assert
        Assert.AreEqual(1, hospede.Veiculos.Count);
        Assert.AreEqual("ABC1234", hospede.Veiculos[0].Placa, true);
    }

    [TestMethod]
    public void Deve_Trocar_Hospede_Do_Veiculo_Quando_Estiver_Vinculado_A_Outro()
    {
        // Arrange
        Hospede hospedeOriginal = new("Original", "11111111111", "(51) 90000-0000");
        hospede = new Hospede("Destino", "22222222222", "(51) 98888-8888");

        Veiculo veiculo = Builder<Veiculo>.CreateNew()
            .With(v => v.Placa = "ABC1234")
            .Build();

        hospedeOriginal.AderirVeiculo(veiculo);

        // Act
        hospede.AderirVeiculo(veiculo);

        // Assert
        Assert.AreEqual(1, hospede.Veiculos.Count);
        Assert.AreEqual(veiculo, hospede.Veiculos[0]);
        Assert.AreEqual(hospede, veiculo.Hospede);
        Assert.AreEqual(hospede.Id, veiculo.HospedeId);
    }

    [TestMethod]
    public void Nao_Deve_Atualizar_Hospede()
    {
        // Arrange
        hospede = new Hospede("Fulano", "12345678909", "(51) 99999-0000");

        Hospede hospedeEditado = Builder<Hospede>.CreateNew()
            .WithFactory(() => new Hospede("Nome Atualizado", "98765432100", "(51) 98888-0000"))
            .Build();

        // Act
        hospede.AtualizarRegistro(null!);

        // Assert
        Assert.AreNotEqual("Nome Atualizado", hospede.NomeCompleto);
        Assert.AreNotEqual("98765432100", hospede.CPF);
        Assert.AreNotEqual("(51) 98888-0000", hospede.Telefone);
    }
}
