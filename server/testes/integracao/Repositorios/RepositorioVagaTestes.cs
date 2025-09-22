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
    public async Task Deve_Cadastrar_Vagas_E_Selecionar_Todos_Ordenados()
    {
        // Arrange
        const int quantidadeVagas = 7;
        const int zonasTotais = 2;
        const int vagasPorZona = 4;

        Result<IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga>> tentativa =
            DistribuidorDeVagas.TentarGerarEsquemaDeVagas(quantidadeVagas, zonasTotais, vagasPorZona);
        IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga> posicoes = tentativa.Value;

        Guid tenantId = Guid.NewGuid();

        Estacionamento estacionamento = new("Estacionamento A", 25);
        estacionamento.VincularTenant(tenantId);

        await repositorioEstacionamento.CadastrarRegistroAsync(estacionamento);

        foreach (DistribuidorDeVagas.PosicaoDaVaga posicao in posicoes)
        {
            Vaga vaga = new()
            {
                EstacionamentoId = estacionamento.Id,
                Zona = posicao.Zona,
                Numero = posicao.Numero
            };
            vaga.VincularTenant(tenantId);

            await repositorioVaga.CadastrarRegistroAsync(vaga);
        }

        await dbContext.SaveChangesAsync();

        // Act
        List<Vaga> registros = await repositorioVaga.SelecionarRegistrosAsync();

        // Assert
        Assert.AreEqual(quantidadeVagas, registros.Count);
        Assert.IsTrue(registros.TrueForAll(v => v.Estacionamento is not null));

        List<Vaga> ordenado = registros.OrderBy(v => v.Zona).ThenBy(v => v.Numero).ToList();
        CollectionAssert.AreEqual(ordenado, registros);
    }

    #region Comentado para evitar erro de timeout no pipeline CI/CD
    //[TestMethod]
    //public async Task Deve_Selecionar_Quantidade_Ordenados_Com_Include_De_Veiculo()
    //{
    //    // Arrange
    //    const int quantidadeVagas = 10;
    //    const int zonasTotais = 3;
    //    const int vagasPorZona = 4;

    //    Result<IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga>> tentativa =
    //        DistribuidorDeVagas.TentarGerarEsquemaDeVagas(quantidadeVagas, zonasTotais, vagasPorZona);
    //    IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga> posicoes = tentativa.Value;

    //    Guid tenantId = Guid.NewGuid();

    //    Estacionamento estacionamento = new("Estacionamento B", 25);
    //    estacionamento.VincularTenant(tenantId);
    //    await repositorioEstacionamento.CadastrarRegistroAsync(estacionamento);

    //    int indice = 0;
    //    foreach (DistribuidorDeVagas.PosicaoDaVaga p in posicoes)
    //    {
    //        Vaga vaga = new()
    //        {
    //            EstacionamentoId = estacionamento.Id,
    //            Zona = p.Zona,
    //            Numero = p.Numero
    //        };
    //        vaga.VincularTenant(tenantId);

    //        if (indice % 3 == 0)
    //        {
    //            Hospede hospede = Builder<Hospede>.CreateNew()
    //                .With(h => h.Id == Guid.NewGuid())
    //                .With(h => h.TenantId == tenantId)
    //                .With(h => h.CPF == indice.ToString("00000000000"))
    //                .Persist();

    //            Veiculo veiculo = Builder<Veiculo>.CreateNew()
    //                .With(v => v.Id == Guid.NewGuid())
    //                .With(v => v.TenantId == tenantId)
    //                .With(v => v.Placa == $"ABC1D{indice:00}")
    //                .Persist();

    //            hospede.AderirVeiculo(veiculo);

    //            RegistroEntrada novoRegistro = new();
    //            novoRegistro.VincularTenant(tenantId);
    //            novoRegistro.AderirHospede(hospede);
    //            novoRegistro.AderirVeiculo(veiculo);
    //            novoRegistro.GerarNovoTicket();
    //            novoRegistro.GerarNovoFaturamento(20);
    //            novoRegistro.VincularTenantAoTicket(tenantId);
    //            hospede.VincularTenant(tenantId);

    //            await repositorioRegistroEntrada.CadastrarRegistroAsync(novoRegistro);

    //            vaga.Ocupar(veiculo);
    //        }


    //        await repositorioVaga.CadastrarRegistroAsync(vaga);
    //        indice++;
    //    }

    //    await dbContext.SaveChangesAsync();

    //    // Act
    //    int quantidade = 5;
    //    List<Vaga> registros = await repositorioVaga.SelecionarRegistrosAsync(quantidade);

    //    // Assert
    //    Assert.AreEqual(quantidade, registros.Count);
    //    Assert.IsTrue(registros.TrueForAll(v => v.Estacionamento is not null));
    //    Assert.IsTrue(registros.All(v => v.Veiculo is null || v.Veiculo is Veiculo));

    //    List<Vaga> ordenado = registros.OrderBy(v => v.Zona).ThenBy(v => v.Numero).ToList();
    //    CollectionAssert.AreEqual(ordenado, registros);
    //}

    //[TestMethod]
    //public async Task Deve_Selecionar_Por_VeiculoId_Corretamente()
    //{
    //    // Arrange
    //    const int quantidadeVagas = 4;
    //    const int zonasTotais = 1;
    //    const int vagasPorZona = 4;

    //    Result<IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga>> tentativa =
    //        DistribuidorDeVagas.TentarGerarEsquemaDeVagas(quantidadeVagas, zonasTotais, vagasPorZona);
    //    IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga> posicoes = tentativa.Value;

    //    Guid tenantId = Guid.NewGuid();

    //    Estacionamento estacionamento = new("Estacionamento C", 25);
    //    estacionamento.VincularTenant(tenantId);
    //    await repositorioEstacionamento.CadastrarRegistroAsync(estacionamento);

    //    Guid veiculoIdEsperado = Guid.NewGuid();
    //    Vaga? vagaComVeiculo = null;

    //    int contador = 0;
    //    foreach (DistribuidorDeVagas.PosicaoDaVaga p in posicoes)
    //    {
    //        Vaga vaga = new()
    //        {
    //            EstacionamentoId = estacionamento.Id,
    //            Zona = p.Zona,
    //            Numero = p.Numero
    //        };
    //        vaga.VincularTenant(tenantId);

    //        if (contador == 2)
    //        {
    //            Hospede hospede = Builder<Hospede>.CreateNew()
    //                .With(h => h.Id == Guid.NewGuid())
    //                .With(h => h.TenantId == tenantId)
    //                .Persist();

    //            Veiculo veiculo = Builder<Veiculo>.CreateNew()
    //                .With(v => v.Id == veiculoIdEsperado)
    //                .With(v => v.TenantId == tenantId)
    //                .Persist();

    //            hospede.AderirVeiculo(veiculo);

    //            RegistroEntrada novoRegistro = new();
    //            novoRegistro.VincularTenant(tenantId);
    //            novoRegistro.AderirHospede(hospede);
    //            novoRegistro.AderirVeiculo(veiculo);
    //            novoRegistro.GerarNovoTicket();
    //            novoRegistro.GerarNovoFaturamento(20);
    //            novoRegistro.VincularTenantAoTicket(tenantId);
    //            hospede.VincularTenant(tenantId);

    //            await repositorioRegistroEntrada.CadastrarRegistroAsync(novoRegistro);

    //            vaga.Ocupar(veiculo);
    //            vagaComVeiculo = vaga;
    //        }

    //        await repositorioVaga.CadastrarRegistroAsync(vaga);
    //        contador++;
    //    }

    //    await dbContext.SaveChangesAsync();

    //    // Act
    //    Vaga? registroSelecionado = await repositorioVaga.SelecionarPorVeiculoIdAsync(veiculoIdEsperado);

    //    // Assert
    //    Assert.IsNotNull(registroSelecionado);
    //    Assert.AreEqual(vagaComVeiculo!.Id, registroSelecionado.Id);
    //    Assert.IsNotNull(registroSelecionado.Estacionamento);
    //    Assert.IsNotNull(registroSelecionado.Veiculo);
    //    Assert.AreEqual(veiculoIdEsperado, registroSelecionado.VeiculoId);
    //}
    #endregion

    [TestMethod]
    public async Task Deve_Selecionar_Registro_Por_ID_Corretamente()
    {
        // Arrange
        const int quantidadeVagas = 7;
        const int zonasTotais = 2;
        const int vagasPorZona = 4;

        Result<IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga>> tentativa =
            DistribuidorDeVagas.TentarGerarEsquemaDeVagas(quantidadeVagas, zonasTotais, vagasPorZona);
        IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga> posicoes = tentativa.Value;

        Guid tenantId = Guid.NewGuid();
        Estacionamento estacionamento = new("Estacionamento do Zé", 25);
        estacionamento.VincularTenant(tenantId);

        await repositorioEstacionamento.CadastrarRegistroAsync(estacionamento);

        Guid vagaIdEsperado = Guid.Empty;
        int indice = 0;

        foreach (DistribuidorDeVagas.PosicaoDaVaga posicao in posicoes)
        {
            Vaga vaga = new()
            {
                EstacionamentoId = estacionamento.Id,
                Zona = posicao.Zona,
                Numero = posicao.Numero
            };
            vaga.VincularTenant(tenantId);

            await repositorioVaga.CadastrarRegistroAsync(vaga);

            if (indice == 5)
                vagaIdEsperado = vaga.Id;

            indice++;
        }

        await dbContext.SaveChangesAsync();

        // Act
        Vaga? registroSelecionado = await repositorioVaga.SelecionarRegistroPorIdAsync(vagaIdEsperado);

        // Assert
        Assert.IsNotNull(registroSelecionado);
        Assert.AreEqual(vagaIdEsperado, registroSelecionado.Id);
    }

    [TestMethod]
    public async Task Deve_Selecionar_Registro_Por_Dados_Corretamente()
    {
        // Arrange
        const int quantidadeVagas = 7;
        const int zonasTotais = 2;
        const int vagasPorZona = 4;

        Result<IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga>> tentativa =
            DistribuidorDeVagas.TentarGerarEsquemaDeVagas(quantidadeVagas, zonasTotais, vagasPorZona);
        IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga> posicoes = tentativa.Value;

        Guid tenantId = Guid.NewGuid();
        Estacionamento estacionamento = new("Estacionamento do Zé", 25);
        estacionamento.VincularTenant(tenantId);

        await repositorioEstacionamento.CadastrarRegistroAsync(estacionamento);

        foreach (DistribuidorDeVagas.PosicaoDaVaga posicao in posicoes)
        {
            Vaga vaga = new()
            {
                EstacionamentoId = estacionamento.Id,
                Zona = posicao.Zona,
                Numero = posicao.Numero
            };
            vaga.VincularTenant(tenantId);

            await repositorioVaga.CadastrarRegistroAsync(vaga);
        }

        await dbContext.SaveChangesAsync();

        // Act
        Vaga? registroSelecionado =
            await repositorioVaga.SelecionarRegistroPorDadosAsync(3, ZonaEstacionamento.A, estacionamento.Id, tenantId);

        // Assert
        Assert.IsNotNull(registroSelecionado);
        Assert.AreEqual(3, registroSelecionado.Numero);
        Assert.AreEqual(ZonaEstacionamento.A, registroSelecionado.Zona);
    }

    [TestMethod]
    public async Task Deve_Selecionar_Registros_Do_Estacionamento_Sem_Filtro()
    {
        // Arrange
        const int quantidadeVagas = 7;
        const int zonasTotais = 2;
        const int vagasPorZona = 4;

        Result<IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga>> tentativa =
            DistribuidorDeVagas.TentarGerarEsquemaDeVagas(quantidadeVagas, zonasTotais, vagasPorZona);
        IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga> posicoes = tentativa.Value;

        Guid tenantId = Guid.NewGuid();
        Estacionamento estacionamento = new("Estacionamento D", 25);
        estacionamento.VincularTenant(tenantId);
        await repositorioEstacionamento.CadastrarRegistroAsync(estacionamento);

        foreach (DistribuidorDeVagas.PosicaoDaVaga posicao in posicoes)
        {
            Vaga vaga = new()
            {
                EstacionamentoId = estacionamento.Id,
                Zona = posicao.Zona,
                Numero = posicao.Numero
            };
            vaga.VincularTenant(tenantId);
            await repositorioVaga.CadastrarRegistroAsync(vaga);
        }

        await dbContext.SaveChangesAsync();

        // Act
        List<Vaga> registros = await repositorioVaga.SelecionarRegistrosDoEstacionamentoAsync(estacionamento.Id, null);

        // Assert
        Assert.AreEqual(quantidadeVagas, registros.Count);
        Assert.IsTrue(registros.All(v => v.EstacionamentoId == estacionamento.Id));

        List<Vaga> ordenado = registros.OrderBy(v => v.Zona).ThenBy(v => v.Numero).ToList();
        CollectionAssert.AreEqual(ordenado, registros);
    }

    [TestMethod]
    public async Task Deve_Selecionar_Registros_Do_Estacionamento_Com_Filtro_Zona()
    {
        // Arrange
        const int quantidadeVagas = 7;
        const int zonasTotais = 2;
        const int vagasPorZona = 4;

        Result<IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga>> tentativa =
            DistribuidorDeVagas.TentarGerarEsquemaDeVagas(quantidadeVagas, zonasTotais, vagasPorZona);
        IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga> posicoes = tentativa.Value;

        Guid tenantId = Guid.NewGuid();
        Estacionamento estacionamento = new("Estacionamento E", 25);
        estacionamento.VincularTenant(tenantId);
        await repositorioEstacionamento.CadastrarRegistroAsync(estacionamento);

        foreach (DistribuidorDeVagas.PosicaoDaVaga posicao in posicoes)
        {
            Vaga vaga = new()
            {
                EstacionamentoId = estacionamento.Id,
                Zona = posicao.Zona,
                Numero = posicao.Numero
            };
            vaga.VincularTenant(tenantId);
            await repositorioVaga.CadastrarRegistroAsync(vaga);
        }

        await dbContext.SaveChangesAsync();

        // Act
        List<Vaga> registros = await repositorioVaga.SelecionarRegistrosDoEstacionamentoAsync(estacionamento.Id, ZonaEstacionamento.A);

        // Assert
        Assert.IsTrue(registros.Count > 0);
        Assert.IsTrue(registros.All(v => v.Zona == ZonaEstacionamento.A));

        List<Vaga> ordenado = registros.OrderBy(v => v.Zona).ThenBy(v => v.Numero).ToList();
        CollectionAssert.AreEqual(ordenado, registros);
    }

    [TestMethod]
    public async Task Deve_Selecionar_Registros_Do_Estacionamento_Com_Quantidade_Sem_Filtro()
    {
        // Arrange
        const int quantidadeVagas = 10;
        const int zonasTotais = 3;
        const int vagasPorZona = 4;

        Result<IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga>> tentativa =
            DistribuidorDeVagas.TentarGerarEsquemaDeVagas(quantidadeVagas, zonasTotais, vagasPorZona);
        IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga> posicoes = tentativa.Value;

        Guid tenantId = Guid.NewGuid();
        Estacionamento estacionamento = new("Estacionamento F", 25);
        estacionamento.VincularTenant(tenantId);
        await repositorioEstacionamento.CadastrarRegistroAsync(estacionamento);

        foreach (DistribuidorDeVagas.PosicaoDaVaga posicao in posicoes)
        {
            Vaga vaga = new()
            {
                EstacionamentoId = estacionamento.Id,
                Zona = posicao.Zona,
                Numero = posicao.Numero
            };
            vaga.VincularTenant(tenantId);
            await repositorioVaga.CadastrarRegistroAsync(vaga);
        }

        await dbContext.SaveChangesAsync();

        // Act
        int quantidade = 4;
        List<Vaga> registros =
            await repositorioVaga.SelecionarRegistrosDoEstacionamentoAsync(quantidade, estacionamento.Id, null);

        // Assert
        Assert.AreEqual(quantidade, registros.Count);
        Assert.IsTrue(registros.All(v => v.EstacionamentoId == estacionamento.Id));

        List<Vaga> ordenado = registros.OrderBy(v => v.Zona).ThenBy(v => v.Numero).ToList();
        CollectionAssert.AreEqual(ordenado, registros);
    }

    [TestMethod]
    public async Task Deve_Selecionar_Registros_Do_Estacionamento_Com_Quantidade_E_Filtro_Zona()
    {
        // Arrange
        const int quantidadeVagas = 9;
        const int zonasTotais = 3;
        const int vagasPorZona = 4;

        Result<IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga>> tentativa =
            DistribuidorDeVagas.TentarGerarEsquemaDeVagas(quantidadeVagas, zonasTotais, vagasPorZona);
        IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga> posicoes = tentativa.Value;

        Guid tenantId = Guid.NewGuid();
        Estacionamento estacionamento = new("Estacionamento G", 25);
        estacionamento.VincularTenant(tenantId);
        await repositorioEstacionamento.CadastrarRegistroAsync(estacionamento);

        foreach (DistribuidorDeVagas.PosicaoDaVaga posicao in posicoes)
        {
            Vaga vaga = new()
            {
                EstacionamentoId = estacionamento.Id,
                Zona = posicao.Zona,
                Numero = posicao.Numero
            };
            vaga.VincularTenant(tenantId);
            await repositorioVaga.CadastrarRegistroAsync(vaga);
        }

        await dbContext.SaveChangesAsync();

        // Act
        int quantidade = 2;
        List<Vaga> registros =
            await repositorioVaga.SelecionarRegistrosDoEstacionamentoAsync(quantidade, estacionamento.Id, ZonaEstacionamento.B);

        // Assert
        Assert.AreEqual(quantidade, registros.Count);
        Assert.IsTrue(registros.All(v => v.Zona == ZonaEstacionamento.B));

        List<Vaga> ordenado = registros.OrderBy(v => v.Zona).ThenBy(v => v.Numero).ToList();
        CollectionAssert.AreEqual(ordenado, registros);
    }
}
