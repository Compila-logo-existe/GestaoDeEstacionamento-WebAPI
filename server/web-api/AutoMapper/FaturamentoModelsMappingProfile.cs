using AutoMapper;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Commands;
using GestaoDeEstacionamento.WebAPI.Models.ModuloFaturamento;

namespace GestaoDeEstacionamento.WebAPI.AutoMapper;

public class FaturamentoModelsMappingProfile : Profile
{
    public FaturamentoModelsMappingProfile()
    {
        #region ObterValorAtual
        CreateMap<ObterValorAtualFaturamentoRequest, ObterValorAtualFaturamentoQuery>();
        CreateMap<ObterValorAtualFaturamentoResult, ObterValorAtualFaturamentoResponse>();
        #endregion
    }
}
