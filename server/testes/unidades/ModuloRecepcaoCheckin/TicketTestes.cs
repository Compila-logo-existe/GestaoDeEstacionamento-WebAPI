using FizzWare.NBuilder;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloRecepcaoCheckin;

[TestClass]
[TestCategory("Testes de Unidade de Ticket (Domínio)")]
public class TicketTestes
{
    private Ticket? ticket;
    private readonly RegistroEntrada registroEntrada = Builder<RegistroEntrada>.CreateNew()
        .WithFactory(() => new(hospede, veiculo, new List<string>())).Build();
    private static readonly Hospede hospede = Builder<Hospede>.CreateNew().Build();
    private static readonly Veiculo veiculo = Builder<Veiculo>.CreateNew().Build();

    [TestMethod]
    public void Deve_Criar_Ticket_Com_Sucesso()
    {
        // Arrange 
        registroEntrada.GerarNovoTicket();

        List<string> observacoes = new() { "Observação 1", "Observação 2" };

        // Act
        ticket = new(registroEntrada);

        // Assert
        Assert.AreEqual(ticket.RegistroEntrada.Hospede, hospede);
        Assert.AreEqual(ticket.RegistroEntrada.Veiculo, veiculo);
    }

    [TestMethod]
    public void Deve_Obter_Data_Entrada_Com_Sucesso()
    {
        // Arrange 
        ticket = new(registroEntrada);

        // Act
        DateTime dataEntrada = ticket.ObterDataEntrada();

        // Assert
        Assert.AreEqual(dataEntrada, registroEntrada.DataEntradaEmUtc);
    }

    [TestMethod]
    public void Deve_Obter_Data_Saida_Com_Sucesso()
    {
        // Arrange 
        ticket = new(registroEntrada);

        RegistroSaida registroSaida = Builder<RegistroSaida>
            .CreateNew()
            .WithFactory(() => new(hospede, veiculo, new List<string>()))
            .Build();

        registroEntrada.Ticket = ticket;
        registroEntrada.GerarNovoFaturamento(5);
        registroSaida.AderirTicket(ticket);

        // Act
        DateTime? dataSaida = ticket.ObterDataSaida();

        // Assert
        Assert.AreEqual(dataSaida, registroSaida.DataSaidaEmUtc);
    }

    [TestMethod]
    public void Deve_Mudar_Status_Para_Expirado_Com_Sucesso()
    {
        // Arrange 
        ticket = new(registroEntrada);

        RegistroSaida registroSaida = Builder<RegistroSaida>
            .CreateNew()
            .WithFactory(() => new(hospede, veiculo, new List<string>()))
            .Build();

        registroEntrada.Ticket = ticket;
        registroEntrada.GerarNovoFaturamento(5);

        // Act;
        registroSaida.AderirTicket(ticket);

        // Assert
        Assert.AreEqual(StatusTicket.Expirado, ticket.Status);
    }

    [TestMethod]
    public void Deve_Mudar_Status_Com_Sucesso()
    {
        // Arrange 
        ticket = new(registroEntrada);

        RegistroSaida registroSaida = Builder<RegistroSaida>
            .CreateNew()
            .WithFactory(() => new(hospede, veiculo, new List<string>()))
            .Build();

        registroEntrada.Ticket = ticket;
        registroEntrada.GerarNovoFaturamento(5);

        // Assert
        Assert.AreEqual(StatusTicket.Valido, ticket.Status);
    }

    [TestMethod]
    public void Deve_Atualizar_Veiculo_Com_Sucesso()
    {
        // Arrange
        ticket = new(registroEntrada);

        Ticket ticketEditado = Builder<Ticket>.CreateNew().Build();

        // Act
        ticket.AtualizarRegistro(ticketEditado);

        // Assert
        Assert.AreEqual(ticket.NumeroSequencial, ticketEditado.NumeroSequencial);
    }

    [TestMethod]
    public void Nao_Deve_Atualizar_Veiculo_Com_Sucesso()
    {
        // Arrange
        ticket = new(registroEntrada);
        Ticket ticketEditado = Builder<Ticket>.CreateNew().Build();

        // Act
        ticket.AtualizarRegistro(null!);

        // Assert
        Assert.AreNotEqual(ticket.NumeroSequencial, ticketEditado.NumeroSequencial);
    }
}
