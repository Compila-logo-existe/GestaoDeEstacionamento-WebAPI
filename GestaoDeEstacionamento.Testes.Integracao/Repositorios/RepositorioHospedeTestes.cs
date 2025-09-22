using FizzWare.NBuilder;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using GestaoDeEstacionamento.Testes.Integracao.Compartilhado;

namespace GestaoDeEstacionamento.Testes.Integracao.Repositorios;

[TestClass]
[TestCategory("Testes de Integração de RepositorioHospede")]
public class RepositorioHospedeTestes : TestFixture
{
    [TestMethod]
    public async Task Deve_Selecionar_Registro_Por_ID_Corretamente()
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
        Hospede? registroSelecionado = await repositorioHospede.SelecionarRegistroPorIdAsync(hospede.Id);

        // Assert
        Assert.IsNotNull(registroSelecionado);
        Assert.AreEqual(hospede, registroSelecionado);
    }

    [TestMethod]
    public async Task Deve_Selecionar_Registro_Por_CPF_Corretamente()
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

        string cpf = hospede.CPF;

        // Act
        Hospede? registroSelecionado = await repositorioHospede.SelecionarRegistroPorCPFAsync(cpf, tenantId);

        // Assert
        Assert.IsNotNull(registroSelecionado);
        Assert.AreEqual(hospede, registroSelecionado);
    }
}
