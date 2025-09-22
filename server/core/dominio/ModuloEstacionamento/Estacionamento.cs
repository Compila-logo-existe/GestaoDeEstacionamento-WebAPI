using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;

public class Estacionamento : EntidadeBase<Estacionamento>
{
    public string Nome { get; set; }
    public List<Vaga> Vagas { get; set; } = new();

    public Estacionamento() { }
    public Estacionamento(string nome, int quantidadeVagas) : this()
    {
        Nome = nome;
        Vagas = new(quantidadeVagas);
    }

    public (IReadOnlyList<Vaga> Vagas, IReadOnlyList<(ZonaEstacionamento Zona, int Quantidade)> Resumo)
    GerarVagas(int quantidadeVagas, int zonasTotais, int vagasPorZona)
    {
        if (quantidadeVagas <= 0) throw new ArgumentOutOfRangeException(nameof(quantidadeVagas));
        if (zonasTotais <= 0 || zonasTotais > 26) throw new ArgumentOutOfRangeException(nameof(zonasTotais));
        if (vagasPorZona <= 0) throw new ArgumentOutOfRangeException(nameof(vagasPorZona));
        if ((long)zonasTotais * vagasPorZona < quantidadeVagas) throw new InvalidOperationException("Capacidade insuficiente.");

        List<Vaga> lista = new(quantidadeVagas);
        List<(ZonaEstacionamento, int)> resumo = new(zonasTotais);

        int restante = quantidadeVagas;

        for (int i = 0; i < zonasTotais && restante > 0; i++)
        {
            ZonaEstacionamento zona = ObterZona(i);
            int alocar = Math.Min(vagasPorZona, restante);

            for (int n = 1; n <= alocar; n++)
            {
                lista.Add(new Vaga
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    EstacionamentoId = Id,
                    Zona = zona,
                    Numero = n
                });
            }
            resumo.Add((zona, alocar));
            restante -= alocar;
        }

        return (lista, resumo);

        static ZonaEstacionamento ObterZona(int i)
        {
            char c = (char)('A' + i);

            return (ZonaEstacionamento)Enum.Parse(typeof(ZonaEstacionamento), c.ToString());
        }
    }

    public override void AtualizarRegistro(Estacionamento registroEditado)
    {
        Vagas = registroEditado.Vagas;
    }
}
