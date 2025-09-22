using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;

namespace GestaoDeEstacionamento.Testes.Unidades.ModuloEstacionamento;

[TestClass]
[TestCategory("Testes de Unidade de Estacionamento (Domínio)")]
public class EstacionamentoTestes
{
    private Estacionamento? estacionamento;

    [TestMethod]
    public void Deve_Criar_Estacionamento_Com_Sucesso()
    {
        // Act
        estacionamento = new Estacionamento("Estacionamento Central", 10);

        // Assert
        Assert.AreEqual("Estacionamento Central", estacionamento.Nome);
        Assert.IsNotNull(estacionamento.Vagas);
        Assert.AreEqual(0, estacionamento.Vagas.Count);
    }

    [TestMethod]
    public void Deve_Gerar_Vagas_Com_Sucesso()
    {
        // Arrange
        estacionamento = new Estacionamento("Pátio A", 0)
        {
            TenantId = Guid.NewGuid(),
            Id = Guid.NewGuid()
        };

        const int quantidadeVagas = 7;
        const int zonasTotais = 2;
        const int vagasPorZona = 4;

        // Act
        (IReadOnlyList<Vaga> vagas, IReadOnlyList<(ZonaEstacionamento Zona, int Quantidade)> resumo) = estacionamento.GerarVagas(quantidadeVagas, zonasTotais, vagasPorZona);

        // Assert
        Assert.AreEqual(quantidadeVagas, vagas.Count);

        Assert.IsTrue(vagas.All(v => v.TenantId == estacionamento.TenantId));
        Assert.IsTrue(vagas.All(v => v.EstacionamentoId == estacionamento.Id));

        Assert.AreEqual(2, resumo.Count);
        Assert.AreEqual((ZonaEstacionamento.A, 4), resumo[0]);
        Assert.AreEqual((ZonaEstacionamento.B, 3), resumo[1]);

        Assert.AreEqual(4, vagas.Count(v => v.Zona == ZonaEstacionamento.A));
        Assert.AreEqual(3, vagas.Count(v => v.Zona == ZonaEstacionamento.B));

        int maxA = vagas.Where(v => v.Zona == ZonaEstacionamento.A).Max(v => v.Numero);
        int maxB = vagas.Where(v => v.Zona == ZonaEstacionamento.B).Max(v => v.Numero);
        Assert.AreEqual(4, maxA);
        Assert.AreEqual(3, maxB);
        Assert.IsTrue(vagas.Any(v => v.Zona == ZonaEstacionamento.A && v.Numero == 1));
        Assert.IsTrue(vagas.Any(v => v.Zona == ZonaEstacionamento.B && v.Numero == 1));
    }

    [TestMethod]
    public void Deve_Gerar_Vagas_Com_Divisao_Exata()
    {
        // Arrange
        estacionamento = new Estacionamento("Pátio B", 0)
        {
            TenantId = Guid.NewGuid(),
            Id = Guid.NewGuid()
        };

        const int quantidadeVagas = 6;
        const int zonasTotais = 3;
        const int vagasPorZona = 2;

        // Act
        (IReadOnlyList<Vaga> vagas, IReadOnlyList<(ZonaEstacionamento Zona, int Quantidade)> resumo) = estacionamento.GerarVagas(quantidadeVagas, zonasTotais, vagasPorZona);

        // Assert
        Assert.AreEqual(6, vagas.Count);
        Assert.AreEqual(3, resumo.Count);
        Assert.AreEqual((ZonaEstacionamento.A, 2), resumo[0]);
        Assert.AreEqual((ZonaEstacionamento.B, 2), resumo[1]);
        Assert.AreEqual((ZonaEstacionamento.C, 2), resumo[2]);

        Assert.AreEqual(2, vagas.Count(v => v.Zona == ZonaEstacionamento.A));
        Assert.AreEqual(2, vagas.Count(v => v.Zona == ZonaEstacionamento.B));
        Assert.AreEqual(2, vagas.Count(v => v.Zona == ZonaEstacionamento.C));

        Assert.IsTrue(vagas.Where(v => v.Zona == ZonaEstacionamento.C).All(v => v.Numero == 1 || v.Numero == 2));
    }

    [TestMethod]
    public void Deve_Lancar_Excecao_Quando_QuantidadeVagas_Invalida()
    {
        // Act
        estacionamento = new Estacionamento("X", 0);

        // Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            estacionamento.GerarVagas(0, 1, 1));
    }

    [TestMethod]
    public void Deve_Lancar_Excecao_Quando_ZonasTotais_Invalido()
    {
        // Act
        estacionamento = new Estacionamento("X", 0);

        // Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            estacionamento.GerarVagas(1, 0, 1));

        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            estacionamento.GerarVagas(1, 27, 1));
    }

    [TestMethod]
    public void Deve_Lancar_Excecao_Quando_VagasPorZona_Invalido()
    {
        // Act
        estacionamento = new Estacionamento("X", 0);

        // Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            estacionamento.GerarVagas(1, 1, 0));
    }

    [TestMethod]
    public void Deve_Lancar_Excecao_Quando_Capacidade_Insuficiente()
    {
        // Act
        estacionamento = new Estacionamento("X", 0);

        // Assert
        InvalidOperationException ex = Assert.ThrowsException<InvalidOperationException>(() =>
            estacionamento.GerarVagas(5, 2, 2));

        Assert.AreEqual("Capacidade insuficiente.", ex.Message);
    }

    [TestMethod]
    public void Deve_Atualizar_Registro_Com_Sucesso()
    {
        // Arrange
        estacionamento = new Estacionamento("A", 0);
        Estacionamento novo = new("B", 0);
        novo.Vagas.Add(new Vaga { Numero = 1, Zona = ZonaEstacionamento.A });

        // Act
        estacionamento.AtualizarRegistro(novo);

        // Assert
        Assert.AreEqual(1, estacionamento.Vagas.Count);
        Assert.AreEqual(novo.Vagas, estacionamento.Vagas);
    }
}
