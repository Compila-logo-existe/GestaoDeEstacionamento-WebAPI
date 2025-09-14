using AutoMapper;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.WebAPI.Models.ModuloRecepcaoCheckin;

namespace GestaoDeEstacionamento.WebAPI.AutoMapper;

public class RecepcaoCheckinModelsMappingProfile : Profile
{
    public RecepcaoCheckinModelsMappingProfile()
    {
        #region Registrar
        CreateMap<RegistrarEntradaRequest, RegistrarEntradaCommand>();
        CreateMap<RegistrarEntradaResult, RegistrarEntradaResponse>();
        #endregion


        #region ObterDetalhesPorId
        CreateMap<Guid, ObterDetalhesVeiculoPorIdQuery>()
            .ConvertUsing(src => new ObterDetalhesVeiculoPorIdQuery(src));
        CreateMap<ObterDetalhesVeiculoPorIdResult, ObterDetalhesVeiculoPorIdResponse>();
        #endregion


        #region ObterDetalhesPorPlaca
        CreateMap<string, ObterDetalhesVeiculoPorPlacaQuery>()
            .ConvertUsing(src => new ObterDetalhesVeiculoPorPlacaQuery(src));
        CreateMap<ObterDetalhesVeiculoPorPlacaResult, ObterDetalhesVeiculoPorPlacaResponse>();
        #endregion
    }
}
