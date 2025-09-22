using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Testes.Integracao.Compartilhado;

namespace GestaoDeEstacionamento.Testes.Integracao.Repositorios;

[TestClass]
[TestCategory("Testes de Integração de RepositorioEstacionamento")]
public class RepositorioEstacionamentoTestes : TestFixture
{
    [TestMethod]
    public async Task Deve_Cadastrar_Vagas_E_Selecionar_Por_Id_Corretamente()
    {
        // Arrange
        Guid tenantId = Guid.NewGuid();
        Estacionamento estacionamento = new("Estacionamento A", 25);
        estacionamento.VincularTenant(tenantId);

        // Act
        await repositorioEstacionamento.CadastrarRegistroAsync(estacionamento);
        await dbContext.SaveChangesAsync();

        // Assert
        Estacionamento? registroSelecionado = await repositorioEstacionamento.SelecionarRegistroPorIdAsync(estacionamento.Id);

        Assert.IsNotNull(registroSelecionado);
        Assert.AreEqual(estacionamento.Id, registroSelecionado.Id);
    }
    [TestMethod]
    public async Task Deve_Cadastrar_Vagas_E_Selecionar_Por_Nome_Corretamente()
    {
        // Arrange
        Guid tenantId = Guid.NewGuid();
        Estacionamento estacionamento = new("Estacionamento A", 25);
        estacionamento.VincularTenant(tenantId);

        // Act
        await repositorioEstacionamento.CadastrarRegistroAsync(estacionamento);
        await dbContext.SaveChangesAsync();

        // Assert
        Estacionamento? registroSelecionado = await repositorioEstacionamento.SelecionarRegistroPorNome("Estacionamento A", tenantId);

        Assert.IsNotNull(registroSelecionado);
        Assert.AreEqual(estacionamento.Id, registroSelecionado.Id);
    }
}
