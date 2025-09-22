using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Testes.Integracao.Compartilhado;

namespace GestaoDeEstacionamento.Testes.Integracao.Repositorios;

[TestClass]
[TestCategory("Testes de Integração de RepositorioVaga")]
public class RepositorioVagaTestes : TestFixture
{
    [TestMethod]
    public async Task Deve_Selecionar_Registro_Por_ID_Corretamente()
    {
        // Arrange
        const int quantidadeVagas = 7;
        const int zonasTotais = 2;
        const int vagasPorZona = 4;

        Result<IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga>> tentativa = DistribuidorDeVagas.TentarGerarEsquemaDeVagas(quantidadeVagas, zonasTotais, vagasPorZona);
        IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga> posicoes = tentativa.Value;

        Guid tenantId = Guid.NewGuid();
        Estacionamento estacionamento = new("Estacionamento do Zé", 25);
        estacionamento.VincularTenant(tenantId);

        // Act
        await repositorioEstacionamento.CadastrarRegistroAsync(estacionamento);

        foreach (DistribuidorDeVagas.PosicaoDaVaga p in posicoes)
        {
            Vaga vaga = new()
            {
                EstacionamentoId = estacionamento.Id,
                Zona = p.Zona,
                Numero = p.Numero
            };
            vaga.VincularTenant(tenantId);

            await repositorioVaga.CadastrarRegistroAsync(vaga);
        }

        await dbContext.SaveChangesAsync();

        Guid vagaId = estacionamento.Vagas[5].Id;

        // Act
        Vaga? registroSelecionado = await repositorioVaga.SelecionarRegistroPorIdAsync(vagaId);

        // Assert
        Assert.IsNotNull(registroSelecionado);
        Assert.AreEqual(vagaId, registroSelecionado.Id);
    }

    [TestMethod]
    public async Task Deve_Selecionar_Registro_Por_Dados_Corretamente()
    {
        // Arrange
        const int quantidadeVagas = 7;
        const int zonasTotais = 2;
        const int vagasPorZona = 4;

        Result<IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga>> tentativa = DistribuidorDeVagas.TentarGerarEsquemaDeVagas(quantidadeVagas, zonasTotais, vagasPorZona);
        IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga> posicoes = tentativa.Value;

        Guid tenantId = Guid.NewGuid();
        Estacionamento estacionamento = new("Estacionamento do Zé", 25);
        estacionamento.VincularTenant(tenantId);

        // Act
        await repositorioEstacionamento.CadastrarRegistroAsync(estacionamento);

        foreach (DistribuidorDeVagas.PosicaoDaVaga p in posicoes)
        {
            Vaga vaga = new()
            {
                EstacionamentoId = estacionamento.Id,
                Zona = p.Zona,
                Numero = p.Numero
            };
            vaga.VincularTenant(tenantId);

            await repositorioVaga.CadastrarRegistroAsync(vaga);
        }

        await dbContext.SaveChangesAsync();

        // Act
        Vaga? registroSelecionado = await repositorioVaga.SelecionarRegistroPorDadosAsync(3, ZonaEstacionamento.A, estacionamento.Id, tenantId);

        // Assert
        Assert.IsNotNull(registroSelecionado);
        Assert.AreEqual(3, registroSelecionado.Numero);
        Assert.AreEqual(ZonaEstacionamento.A, registroSelecionado.Zona);
    }
}
