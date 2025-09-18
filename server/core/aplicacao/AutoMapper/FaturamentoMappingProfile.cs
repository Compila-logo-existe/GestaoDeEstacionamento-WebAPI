using AutoMapper;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloFaturamento;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Core.Aplicacao.AutoMapper;

public class FaturamentoMappingProfile : Profile
{
    public FaturamentoMappingProfile()
    {

        #region ObterValorAtual
        CreateMap<(int numeroDiarias, decimal valorDiaria, decimal valorTotal), ObterValorAtualFaturamentoResult>()
            .ConvertUsing(src => new ObterValorAtualFaturamentoResult(
                src.numeroDiarias,
                src.valorDiaria,
                src.valorTotal
                )
            );
        #endregion

        #region GerarRelatorio
        CreateMap<Faturamento, FaturamentoDto>()
           .ConvertUsing(src =>
               new FaturamentoDto(
                    src.Id,
                    src.RegistroEntrada.Id,
                    src.RegistroSaida != null ? src.RegistroSaida.Id : Guid.Empty,
                    src.DataEntradaEmUtc,
                    src.RegistroSaida != null ? src.RegistroSaida.DataSaidaEmUtc : null,
                    src.ValorDaDiaria,
                    src.NumeroDeDiarias,
                    src.ValorTotal
                )
           );

        CreateMap<(GerarRelatorioFinanceiroQuery, List<Faturamento>, decimal), GerarRelatorioFinanceiroResult>()
            .ConvertUsing((src, dest, ctx) => new GerarRelatorioFinanceiroResult(
                src.Item1.DataInicial,
                src.Item1.DataFinal,
                src.Item2.Count,
                src.Item3,
                src.Item2.Select(c => ctx.Mapper.Map<FaturamentoDto>(c)).ToImmutableList() ??
                    ImmutableList<FaturamentoDto>.Empty
                )
            );
        #endregion
    }
}
