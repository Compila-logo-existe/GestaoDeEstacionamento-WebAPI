using FizzWare.NBuilder;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloRecepcaoCheckin;

[TestClass]
[TestCategory("Testes de Unidade de RegistrarSaida (Domínio)")]
public class RegistroSaidaTestes
{
    private RegistroSaida? registroSaida;
    private readonly RegistroEntrada registroEntrada = Builder<RegistroEntrada>.CreateNew()
        .WithFactory(() => new(hospede, veiculo, new List<string>())).Build();
    private static readonly Hospede hospede = Builder<Hospede>.CreateNew().Build();
    private static readonly Veiculo veiculo = Builder<Veiculo>.CreateNew().Build();

    [TestMethod]
    public void Deve_Criar_Registro_Com_Sucesso()
    {
        // Arrange 
        List<string> observacoes = new() { "Observação 1", "Observação 2" };

        // Act
        RegistroSaida novoRegistro = Builder<RegistroSaida>
            .CreateNew()
            .WithFactory(() => new(hospede, veiculo, observacoes))
            .Build();

        // Assert
        Assert.AreEqual(novoRegistro.Hospede, hospede);
        Assert.AreEqual(novoRegistro.Veiculo, veiculo);
        CollectionAssert.AreEquivalent(novoRegistro.Observacoes, observacoes);
    }

    [TestMethod]
    public void Deve_Aderir_Hospede_Com_Sucesso()
    {
        // Arrange 
        registroSaida = new(null!, veiculo, new List<string>());

        // Act
        registroSaida.AderirHospede(hospede);

        // Assert
        Assert.AreEqual(registroSaida.Hospede, hospede);
    }

    [TestMethod]
    public void Deve_Aderir_Veiculo_Com_Sucesso()
    {
        // Arrange 
        registroSaida = new(hospede, null!, new List<string>());

        // Act
        registroSaida.AderirVeiculo(veiculo);

        // Assert
        Assert.AreEqual(registroSaida.Veiculo, veiculo);
    }

    [TestMethod]
    public void Deve_Aderir_Ticket_Com_Sucesso()
    {
        // Arrange 
        registroSaida = new(hospede, veiculo, new List<string>());
        registroEntrada.GerarNovoTicket();

        // Act
        registroSaida.AderirTicket(registroEntrada.Ticket);

        // Assert
        Assert.IsNotNull(registroSaida.Ticket);
    }

    [TestMethod]
    public void Deve_Atualizar_Registro_Com_Sucesso()
    {
        // Arrange
        registroSaida = new(hospede, veiculo, new List<string>());
        RegistroSaida registroEditado = Builder<RegistroSaida>
            .CreateNew()
            .With(r => r.Observacoes = new List<string> { "Observação 1", "Observação 2" })
            .Build();

        registroSaida.AderirHospede(hospede);
        registroSaida.AderirVeiculo(veiculo);
        registroEntrada.GerarNovoTicket();
        registroSaida.AderirTicket(registroEntrada.Ticket);

        // Act
        registroSaida.AtualizarRegistro(registroEditado);

        // Assert
        Assert.AreEqual(registroSaida.Observacoes, registroEditado.Observacoes);
    }

    [TestMethod]
    public void Nao_Deve_Atualizar_Registro_Com_Sucesso()
    {
        // Arrange
        registroSaida = new(hospede, veiculo, new List<string>());

        // Act
        registroSaida.AtualizarRegistro(null!);

        // Assert
        Assert.IsTrue(registroSaida.Observacoes.Count <= 0);
    }
}
