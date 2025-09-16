using FluentResults;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;

namespace GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;

public static class DistribuidorDeVagas
{
    public sealed record PosicaoDaVaga(ZonaEstacionamento Zona, int Numero);

    public static Result<IReadOnlyList<PosicaoDaVaga>> TentarGerarEsquemaDeVagas(
        int quantidadeDeVagas,
        int quantidadeTotalDeZonas,
        int vagasPorZona)
    {
        List<IError> erros = new();

        if (quantidadeDeVagas <= 0)
            erros.Add(new Error("QuantidadeVagas deve ser maior que zero."));

        if (quantidadeTotalDeZonas <= 0)
            erros.Add(new Error("ZonasTotais deve ser maior que zero."));

        int zonasSuportadas = Enum.GetValues<ZonaEstacionamento>().Length;
        if (quantidadeTotalDeZonas > zonasSuportadas)
            erros.Add(new Error($"ZonasTotais não pode ser maior que {zonasSuportadas} (A..Z)."));

        if (vagasPorZona <= 0)
            erros.Add(new Error("VagasPorZona deve ser maior que zero."));

        long capacidade = (long)quantidadeTotalDeZonas * vagasPorZona;
        if (capacidade < quantidadeDeVagas)
            erros.Add(new Error("Capacidade insuficiente: ZonasTotais * VagasPorZona deve ser >= QuantidadeVagas."));

        if (erros.Count > 0)
            return Result.Fail(erros);

        List<PosicaoDaVaga> posicoes = new(quantidadeDeVagas);

        int vagasRestantes = quantidadeDeVagas;
        for (int i = 0; i < quantidadeTotalDeZonas && vagasRestantes > 0; i++)
        {
            ZonaEstacionamento zona = (ZonaEstacionamento)i;
            int alocar = vagasRestantes >= vagasPorZona ? vagasPorZona : vagasRestantes;

            for (int numero = 1; numero <= alocar; numero++)
                posicoes.Add(new PosicaoDaVaga(zona, numero));

            vagasRestantes -= alocar;
        }

        return Result.Ok((IReadOnlyList<PosicaoDaVaga>)posicoes);
    }
}
