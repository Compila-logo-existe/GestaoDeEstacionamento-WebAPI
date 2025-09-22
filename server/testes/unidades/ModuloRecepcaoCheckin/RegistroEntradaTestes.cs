using FizzWare.NBuilder;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloRecepcaoCheckin;

[TestClass]
[TestCategory("Testes de Unidade de RegistrarEntrada (Domínio)")]
public class RegistroEntradaTestes
{
    private RegistroEntrada? registroEntrada;
    private readonly Hospede hospede = Builder<Hospede>.CreateNew().Build();
    private readonly Veiculo veiculo = Builder<Veiculo>.CreateNew().Build();

    [TestMethod]
    public void Deve_Criar_Registro_Com_Sucesso()
    {
        // Arrange 
        List<string> observacoes = new() { "Observação 1", "Observação 2" };

        // Act
        RegistroEntrada novoRegistro = Builder<RegistroEntrada>
            .CreateNew()
            .WithFactory(() => new(hospede, veiculo, observacoes))
            .Build();

        // Assert
        Assert.AreEqual(novoRegistro.Hospede, hospede);
        Assert.AreEqual(novoRegistro.Veiculo, veiculo);
        CollectionAssert.AreEquivalent(novoRegistro.Observacoes, observacoes);
    }

    [TestMethod]
    public void Deve_Vincular_Tenant_Ao_Ticket_Com_Sucesso()
    {
        // Arrange 
        Tenant tenant = Builder<Tenant>.CreateNew().With(t => t.Id == Guid.NewGuid()).Build();
        registroEntrada = new(hospede, veiculo, new List<string>());
        registroEntrada.GerarNovoTicket();

        // Act
        registroEntrada.VincularTenantAoTicket(tenant.Id);

        // Assert
        Assert.AreEqual(registroEntrada.Ticket.TenantId, tenant.Id);
    }

    [TestMethod]
    public void Deve_Aderir_Hospede_Com_Sucesso()
    {
        // Arrange 
        registroEntrada = new(null!, veiculo, new List<string>());
        registroEntrada.GerarNovoTicket();

        // Act
        registroEntrada.AderirHospede(hospede);

        // Assert
        Assert.AreEqual(registroEntrada.Hospede, hospede);
    }

    [TestMethod]
    public void Deve_Aderir_Veiculo_Com_Sucesso()
    {
        // Arrange 
        registroEntrada = new(hospede, null!, new List<string>());
        registroEntrada.GerarNovoTicket();

        // Act
        registroEntrada.AderirVeiculo(veiculo);

        // Assert
        Assert.AreEqual(registroEntrada.Veiculo, veiculo);
    }

    [TestMethod]
    public void Deve_Gerar_Ticket_Com_Sucesso()
    {
        // Arrange 
        registroEntrada = new(hospede, veiculo, new List<string>());

        // Act
        registroEntrada.GerarNovoTicket();

        // Assert
        Assert.IsNotNull(registroEntrada.Ticket);
    }

    [TestMethod]
    public void Deve_Aderir_Data_Saida_Com_Sucesso()
    {
        // Arrange 
        registroEntrada = new(hospede, veiculo, new List<string>());

        // Act
        registroEntrada.AderirDataSaida(DateTime.UtcNow);

        // Assert
        Assert.IsNotNull(registroEntrada.DataSaidaEmUtc);
    }

    [TestMethod]
    public void Deve_Atualizar_Registro_Com_Sucesso()
    {
        // Arrange
        registroEntrada = new(hospede, veiculo, new List<string>());
        RegistroEntrada registroEditado = Builder<RegistroEntrada>
            .CreateNew()
            .With(r => r.Observacoes = new List<string> { "Observação 1", "Observação 2" })
            .Build();

        // Act
        registroEntrada.AtualizarRegistro(registroEditado);

        // Assert
        Assert.AreEqual(registroEntrada.Observacoes, registroEditado.Observacoes);
    }

    [TestMethod]
    public void Nao_Deve_Atualizar_Registro_Com_Sucesso()
    {
        // Arrange
        registroEntrada = new(hospede, veiculo, new List<string>());

        // Act
        registroEntrada.AtualizarRegistro(null!);

        // Assert
        Assert.IsTrue(registroEntrada.Observacoes.Count <= 0);
    }
}
