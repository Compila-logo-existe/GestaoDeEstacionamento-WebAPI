using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de Unidade de Tenant (Domínio)")]
public class TenantTestes
{
    private Tenant? tenant;

    [TestMethod]
    public void Deve_Desativar_Com_Sucesso()
    {
        // Arrange
        tenant = new(Guid.NewGuid(), "Empresa Teste", "12.345.678/0001-90",
            "empresa-teste", "www.empresa-teste.com", DateTime.UtcNow);

        // Act
        tenant.Desativar();

        // Assert
        Assert.IsFalse(tenant.Ativo);
    }

    [TestMethod]
    public void Deve_Atualizar_Tenant_Com_Sucesso()
    {
        // Arrange
        tenant = new Tenant(Guid.NewGuid(), "Empresa Teste", "12.345.678/0001-90",
            "empresa-teste", "www.empresa-teste.com", DateTime.UtcNow);

        Tenant tenantEditado = new(Guid.NewGuid(), "Empresa Atualizada", "98.765.432/0001-09",
            "empresa-atualizada", "www.empresa-atualizada.com", DateTime.UtcNow);

        // Act
        tenant.AtualizarRegistro(tenantEditado);

        // Assert
        Assert.AreEqual(tenantEditado.Nome, tenant.Nome);
        Assert.AreEqual(tenantEditado.CNPJ, tenant.CNPJ);
        Assert.AreEqual(tenantEditado.SlugSubdominio, tenant.SlugSubdominio);
        Assert.AreEqual(tenantEditado.DominioPersonalizado, tenant.DominioPersonalizado);
    }

    [TestMethod]
    public void Nao_Deve_Atualizar_Tenant()
    {
        // Arrange
        tenant = new Tenant(Guid.NewGuid(), "Empresa Teste", "12.345.678/0001-90",
            "empresa-teste", "www.empresa-teste.com", DateTime.UtcNow);

        Tenant tenantEditado = new(Guid.NewGuid(), "Empresa Atualizada", "98.765.432/0001-09",
            "empresa-atualizada", "www.empresa-atualizada.com", DateTime.UtcNow);

        // Act
        tenant.AtualizarRegistro(null!);

        // Assert
        Assert.AreNotEqual(tenantEditado.Nome, tenant.Nome);
        Assert.AreNotEqual(tenantEditado.CNPJ, tenant.CNPJ);
        Assert.AreNotEqual(tenantEditado.SlugSubdominio, tenant.SlugSubdominio);
        Assert.AreNotEqual(tenantEditado.DominioPersonalizado, tenant.DominioPersonalizado);
    }
}
