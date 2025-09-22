using FizzWare.NBuilder;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using GestaoDeEstacionamento.Testes.Integracao.Compartilhado;

namespace GestaoDeEstacionamento.Testes.Integracao.Repositorios;

[TestClass]
[TestCategory("Testes de Integração de RepositorioRegistroSaida")]
public class RepositorioRegistroSaidaTestes : TestFixture
{
    [TestMethod]
    public async Task Deve_Cadastrar_RegistroSaida_E_Selecionar_Corretamente()
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

        await repositorioRegistroEntrada.CadastrarRegistroAsync(novoRegistro);
        await dbContext.SaveChangesAsync();

        RegistroSaida novoRegistroSaida = new();
        novoRegistroSaida.VincularTenant(tenantId);
        novoRegistroSaida.AderirHospede(hospede);
        novoRegistroSaida.AderirVeiculo(veiculo);
        novoRegistroSaida.AderirTicket(novoRegistro.Ticket!);

        // Act
        await repositorioRegistroSaida.CadastrarRegistroAsync(novoRegistroSaida);
        await dbContext.SaveChangesAsync();

        // Assert
        RegistroSaida? registroSelecionado = await repositorioRegistroSaida.SelecionarRegistroPorIdAsync(novoRegistroSaida.Id);

        Assert.IsNotNull(registroSelecionado);
        Assert.AreEqual(novoRegistroSaida, registroSelecionado);
        Assert.AreEqual(hospede, registroSelecionado.Hospede);
        Assert.AreEqual(veiculo, registroSelecionado.Veiculo);
    }
}
