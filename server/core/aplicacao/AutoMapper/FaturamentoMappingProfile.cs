using AutoMapper;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Commands;

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
    }
}
