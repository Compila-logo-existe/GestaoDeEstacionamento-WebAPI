using FizzWare.NBuilder;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using GestaoDeEstacionamento.Testes.Integracao.Compartilhado;

namespace GestaoDeEstacionamento.Testes.Integracao.Repositorios;

[TestClass]
[TestCategory("Testes de Integração de RepositorioRegistroEntrada")]
public class RepositorioRegistroEntradaTestes : TestFixture
{
    [TestMethod]
    public async Task Deve_Cadastrar_RegistroEntrada_E_Selecionar_Corretamente()
    {
        // Arrange
        Guid tenantId = Guid.NewGuid();
        Hospede hospede = Builder<Hospede>.CreateNew().With(h => h.Id == Guid.NewGuid()).With(h => h.TenantId == tenantId).Persist();
        Veiculo veiculo = Builder<Veiculo>.CreateNew().With(v => v.Id == Guid.NewGuid()).With(v => v.TenantId == tenantId).Persist();
        hospede.AderirVeiculo(veiculo);
        RegistroEntrada novoRegistro = new();
        novoRegistro.VincularTenant(tenantId);
        novoRegistro.AderirHospede(hospede);
        novoRegistro.AderirVeiculo(veiculo);
        novoRegistro.GerarNovoTicket();
        novoRegistro.GerarNovoFaturamento(20);
        novoRegistro.VincularTenantAoTicket(tenantId);
        hospede.VincularTenant(tenantId);

        // Act
        await repositorioRegistroEntrada.CadastrarRegistroAsync(novoRegistro);

        await dbContext.SaveChangesAsync();

        // Assert
        RegistroEntrada? registroSelecionado = await repositorioRegistroEntrada.SelecionarRegistroPorIdAsync(novoRegistro.Id);

        Assert.IsNotNull(registroSelecionado);
        Assert.AreEqual(novoRegistro, registroSelecionado);
        Assert.AreEqual(hospede, registroSelecionado.Hospede);
        Assert.AreEqual(veiculo, registroSelecionado.Veiculo);
    }

    [TestMethod]
    public async Task Deve_Selecionar_Todos_Registros_Do_Veiculo_Corretamente()
    {
        // Arrange
        Guid tenantId = Guid.NewGuid();
        Hospede hospede = Builder<Hospede>.CreateNew().With(h => h.Id == Guid.NewGuid()).With(h => h.TenantId == tenantId).Persist();
        Veiculo veiculo = Builder<Veiculo>.CreateNew().With(v => v.Id == Guid.NewGuid()).With(v => v.TenantId == tenantId).Persist();
        hospede.AderirVeiculo(veiculo);
        RegistroEntrada novoRegistro = new();
        novoRegistro.VincularTenant(tenantId);
        novoRegistro.AderirHospede(hospede);
        novoRegistro.AderirVeiculo(veiculo);
        novoRegistro.GerarNovoTicket();
        novoRegistro.GerarNovoFaturamento(20);
        novoRegistro.VincularTenantAoTicket(tenantId);
        hospede.VincularTenant(tenantId);

        await repositorioHospede.CadastrarRegistroAsync(hospede);
        await repositorioVeiculo.CadastrarRegistroAsync(veiculo);
        await repositorioRegistroEntrada.CadastrarRegistroAsync(novoRegistro);
        await dbContext.SaveChangesAsync();

        // Act
        List<RegistroEntrada> registros = await repositorioRegistroEntrada.SelecionarRegistrosDoVeiculoAsync(veiculo.Id);

        // Assert
        Assert.IsNotNull(registros);
        Assert.IsTrue(registros.Count >= 1);
        Assert.AreEqual(veiculo, registros[0].Veiculo);
    }

    [TestMethod]
    public async Task Deve_Selecionar_Dois_Registros_Do_Veiculo_Corretamente()
    {
        // Arrange
        Guid tenantId = Guid.NewGuid();
        Hospede hospede = Builder<Hospede>.CreateNew().With(h => h.Id == Guid.NewGuid()).With(h => h.TenantId == tenantId).Persist();
        Veiculo veiculo = Builder<Veiculo>.CreateNew().With(v => v.Id == Guid.NewGuid()).With(v => v.TenantId == tenantId).Persist();
        hospede.AderirVeiculo(veiculo);
        RegistroEntrada novoRegistro = new();
        novoRegistro.VincularTenant(tenantId);
        novoRegistro.AderirHospede(hospede);
        novoRegistro.AderirVeiculo(veiculo);
        novoRegistro.GerarNovoTicket();
        novoRegistro.GerarNovoFaturamento(20);
        novoRegistro.VincularTenantAoTicket(tenantId);
        hospede.VincularTenant(tenantId);

        await repositorioHospede.CadastrarRegistroAsync(hospede);
        await repositorioVeiculo.CadastrarRegistroAsync(veiculo);
        await repositorioRegistroEntrada.CadastrarRegistroAsync(novoRegistro);
        await dbContext.SaveChangesAsync();

        // Act
        List<RegistroEntrada> registros = await repositorioRegistroEntrada.SelecionarRegistrosDoVeiculoAsync(1, veiculo.Id);

        // Assert
        Assert.IsNotNull(registros);
        Assert.IsTrue(registros.Count >= 1);
        Assert.AreEqual(veiculo, registros[0].Veiculo);
    }

    [TestMethod]
    public async Task Deve_Selecionar_Todos_RegistrosCorretamente()
    {
        // Arrange
        Guid tenantId = Guid.NewGuid();
        Hospede hospede = Builder<Hospede>.CreateNew().With(h => h.Id == Guid.NewGuid()).With(h => h.TenantId == tenantId).Persist();
        Veiculo veiculo = Builder<Veiculo>.CreateNew().With(v => v.Id == Guid.NewGuid()).With(v => v.TenantId == tenantId).Persist();
        hospede.AderirVeiculo(veiculo);
        RegistroEntrada novoRegistro = new();
        novoRegistro.VincularTenant(tenantId);
        novoRegistro.AderirHospede(hospede);
        novoRegistro.AderirVeiculo(veiculo);
        novoRegistro.GerarNovoTicket();
        novoRegistro.GerarNovoFaturamento(20);
        novoRegistro.VincularTenantAoTicket(tenantId);
        hospede.VincularTenant(tenantId);

        await repositorioHospede.CadastrarRegistroAsync(hospede);
        await repositorioVeiculo.CadastrarRegistroAsync(veiculo);
        await repositorioRegistroEntrada.CadastrarRegistroAsync(novoRegistro);
        await dbContext.SaveChangesAsync();

        // Act
        List<RegistroEntrada> registros = await repositorioRegistroEntrada.SelecionarRegistrosAsync();

        // Assert
        Assert.IsNotNull(registros);
        Assert.IsTrue(registros.Count >= 1);
    }

    [TestMethod]
    public async Task Deve_Selecionar_Dois_Registros_Corretamente()
    {
        // Arrange
        Guid tenantId = Guid.NewGuid();
        Hospede hospede = Builder<Hospede>.CreateNew().With(h => h.Id == Guid.NewGuid()).With(h => h.TenantId == tenantId).Persist();
        Veiculo veiculo = Builder<Veiculo>.CreateNew().With(v => v.Id == Guid.NewGuid()).With(v => v.TenantId == tenantId).Persist();
        hospede.AderirVeiculo(veiculo);
        RegistroEntrada novoRegistro = new();
        novoRegistro.VincularTenant(tenantId);
        novoRegistro.AderirHospede(hospede);
        novoRegistro.AderirVeiculo(veiculo);
        novoRegistro.GerarNovoTicket();
        novoRegistro.GerarNovoFaturamento(20);
        novoRegistro.VincularTenantAoTicket(tenantId);
        hospede.VincularTenant(tenantId);

        await repositorioHospede.CadastrarRegistroAsync(hospede);
        await repositorioVeiculo.CadastrarRegistroAsync(veiculo);
        await repositorioRegistroEntrada.CadastrarRegistroAsync(novoRegistro);
        await dbContext.SaveChangesAsync();

        // Act
        List<RegistroEntrada> registros = await repositorioRegistroEntrada.SelecionarRegistrosAsync(1);

        // Assert
        Assert.IsNotNull(registros);
        Assert.IsTrue(registros.Count >= 1);
    }

    [TestMethod]
    public async Task Deve_Verificar_Se_Existe_Abertura_Por_Placa_Corretamente()
    {
        // Arrange
        Guid tenantId = Guid.NewGuid();
        Hospede hospede = Builder<Hospede>.CreateNew().With(h => h.Id == Guid.NewGuid()).With(h => h.TenantId == tenantId).Persist();
        Veiculo veiculo = Builder<Veiculo>.CreateNew().With(v => v.Id == Guid.NewGuid()).With(v => v.TenantId == tenantId).Persist();
        hospede.AderirVeiculo(veiculo);
        RegistroEntrada novoRegistro = new();
        novoRegistro.VincularTenant(tenantId);
        novoRegistro.AderirHospede(hospede);
        novoRegistro.AderirVeiculo(veiculo);
        novoRegistro.GerarNovoTicket();
        novoRegistro.GerarNovoFaturamento(20);
        novoRegistro.VincularTenantAoTicket(tenantId);
        hospede.VincularTenant(tenantId);

        await repositorioHospede.CadastrarRegistroAsync(hospede);
        await repositorioVeiculo.CadastrarRegistroAsync(veiculo);
        await repositorioRegistroEntrada.CadastrarRegistroAsync(novoRegistro);
        await dbContext.SaveChangesAsync();

        // Act
        bool existeAbertura = await repositorioRegistroEntrada.ExisteAberturaPorPlacaAsync(veiculo.Placa, tenantId);

        // Assert
        Assert.IsTrue(existeAbertura);
    }

    [TestMethod]
    public async Task Deve_Selecionar_RegistroEntrada_Por_Numero_Ticket_Corretamente()
    {
        // Arrange
        Guid tenantId = Guid.NewGuid();
        Hospede hospede = Builder<Hospede>.CreateNew().With(h => h.Id == Guid.NewGuid()).With(h => h.TenantId == tenantId).Persist();
        Veiculo veiculo = Builder<Veiculo>.CreateNew().With(v => v.Id == Guid.NewGuid()).With(v => v.TenantId == tenantId).Persist();
        hospede.AderirVeiculo(veiculo);
        RegistroEntrada novoRegistro = new();
        novoRegistro.VincularTenant(tenantId);
        novoRegistro.AderirHospede(hospede);
        novoRegistro.AderirVeiculo(veiculo);
        novoRegistro.GerarNovoTicket();
        novoRegistro.GerarNovoFaturamento(20);
        novoRegistro.VincularTenantAoTicket(tenantId);
        hospede.VincularTenant(tenantId);

        await repositorioHospede.CadastrarRegistroAsync(hospede);
        await repositorioVeiculo.CadastrarRegistroAsync(veiculo);
        await repositorioRegistroEntrada.CadastrarRegistroAsync(novoRegistro);
        await dbContext.SaveChangesAsync();

        int numeroTicket = novoRegistro.Ticket.NumeroSequencial;

        // Act
        RegistroEntrada? registroSelecionado = await repositorioRegistroEntrada.SelecionarAberturaPorNumeroDoTicketAsync(numeroTicket, tenantId);

        // Assert
        Assert.IsNotNull(registroSelecionado);
        Assert.AreEqual(novoRegistro, registroSelecionado);
        Assert.AreEqual(hospede, registroSelecionado.Hospede);
        Assert.AreEqual(veiculo, registroSelecionado.Veiculo);
    }

    [TestMethod]
    public async Task Deve_Selecionar_RegistroEntrada_Por_Placa_Veiculo_Corretamente()
    {
        // Arrange
        Guid tenantId = Guid.NewGuid();
        Hospede hospede = Builder<Hospede>.CreateNew().With(h => h.Id == Guid.NewGuid()).With(h => h.TenantId == tenantId).Persist();
        Veiculo veiculo = Builder<Veiculo>.CreateNew().With(v => v.Id == Guid.NewGuid()).With(v => v.TenantId == tenantId).Persist();
        hospede.AderirVeiculo(veiculo);
        RegistroEntrada novoRegistro = new();
        novoRegistro.VincularTenant(tenantId);
        novoRegistro.AderirHospede(hospede);
        novoRegistro.AderirVeiculo(veiculo);
        novoRegistro.GerarNovoTicket();
        novoRegistro.GerarNovoFaturamento(20);
        novoRegistro.VincularTenantAoTicket(tenantId);
        hospede.VincularTenant(tenantId);
        string placa = veiculo.Placa;

        // Act
        await repositorioRegistroEntrada.CadastrarRegistroAsync(novoRegistro);

        await dbContext.SaveChangesAsync();

        // Assert
        RegistroEntrada? registroSelecionado = await repositorioRegistroEntrada.SelecionarAberturaPorPlacaAsync(placa, tenantId);

        Assert.IsNotNull(registroSelecionado);
        Assert.AreEqual(novoRegistro, registroSelecionado);
        Assert.AreEqual(hospede, registroSelecionado.Hospede);
        Assert.AreEqual(veiculo, registroSelecionado.Veiculo);
    }
}
