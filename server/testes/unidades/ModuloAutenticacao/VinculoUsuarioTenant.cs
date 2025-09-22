using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloAutenticacao;

[TestClass]
[TestCategory("Testes de Unidade de VinculoUsuarioTenant (Domínio)")]
public class VinculoUsuarioTenantTestes
{
    private VinculoUsuarioTenant? vinculo;

    [TestMethod]
    public void Deve_Criar_VinculoUsuarioTenant_Com_Sucesso()
    {
        // Arrange
        Guid usuarioVinculadoIdEsperado = Guid.NewGuid();
        Guid tenantIdEsperado = Guid.NewGuid();
        const string nomeCargoEsperado = "Admin";
        const string slugEsperado = "Empresa Teste";

        // Act
        vinculo = new VinculoUsuarioTenant(
            usuarioVinculadoIdEsperado,
            tenantIdEsperado,
            nomeCargoEsperado,
            slugEsperado
        );

        // Assert
        Assert.AreEqual(usuarioVinculadoIdEsperado, vinculo.UsuarioVinculadoId);
        Assert.AreEqual(tenantIdEsperado, vinculo.TenantId);
        Assert.AreEqual(nomeCargoEsperado, vinculo.NomeCargo);
        Assert.AreEqual(slugEsperado, vinculo.Slug);
    }

    [TestMethod]
    public void Deve_Vincular_Tenant_Com_Sucesso()
    {
        // Arrange
        vinculo = new VinculoUsuarioTenant(
            Guid.NewGuid(),
            Guid.Empty,
            "Admin",
            "Empresa Teste"
        );
        Guid tenantIdEsperado = Guid.NewGuid();

        // Act
        vinculo.VincularTenant(tenantIdEsperado);

        // Assert
        Assert.AreEqual(tenantIdEsperado, vinculo.TenantId);
    }

    [TestMethod]
    public void Deve_Atualizar_Registro_Com_Sucesso()
    {
        // Arrange
        vinculo = new VinculoUsuarioTenant(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Ademin",
            "Empresa Teste"
        );

        VinculoUsuarioTenant vinculoEditado = new(
            vinculo.UsuarioVinculadoId,
            vinculo.TenantId,
            "Gerente",
            "Empresa Teste 2"
        );

        // Act
        vinculo.AtualizarRegistro(vinculoEditado);

        // Assert
        Assert.AreEqual("Gerente", vinculo.NomeCargo);
    }

    [TestMethod]
    public void Nao_Deve_Atualizar_Registro()
    {
        // Arrange
        vinculo = new VinculoUsuarioTenant(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Ademin",
            "Empresa Teste"
        );

        VinculoUsuarioTenant vinculoEditado = new(
            vinculo.UsuarioVinculadoId,
            vinculo.TenantId,
            "Gerente",
            "Empresa Teste 2"
        );

        // Act
        vinculo.AtualizarRegistro(null!);

        // Assert
        Assert.AreNotEqual("Gerente", vinculo.NomeCargo);
    }
}
