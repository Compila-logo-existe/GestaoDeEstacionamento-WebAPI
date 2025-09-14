using AutoMapper;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.WebAPI.Models.ModuloRecepcaoCheckin;
using System.Collections.Immutable;

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

        #region SeleçãoTodosRegistros
        CreateMap<SelecionarRegistrosEntradaRequest, SelecionarRegistrosEntradaQuery>();
        CreateMap<SelecionarRegistrosEntradaResult, SelecionarRegistrosEntradaResponse>()
            .ConvertUsing((src, dest, ctx) =>
            new SelecionarRegistrosEntradaResponse(
                src.RegistrosEntrada.Count,
                src?.RegistrosEntrada.Select(c => ctx.Mapper.Map<SelecionarRegistrosEntradaDto>(c)).ToImmutableList() ??
                ImmutableList<SelecionarRegistrosEntradaDto>.Empty
            ));
        #endregion

        #region SeleçãoTodosRegistrosDoVeiculo
        CreateMap<SelecionarRegistrosDoVeiculoRequest, SelecionarRegistrosDoVeiculoQuery>();
        CreateMap<SelecionarRegistrosDoVeiculoResult, SelecionarRegistrosDoVeiculoResponse>()
            .ConvertUsing((src, dest, ctx) =>
            new SelecionarRegistrosDoVeiculoResponse(
                src.RegistrosEntrada.Count,
                src?.RegistrosEntrada.Select(c => ctx.Mapper.Map<SelecionarRegistrosEntradaDto>(c)).ToImmutableList() ??
                ImmutableList<SelecionarRegistrosEntradaDto>.Empty
            ));
        #endregion
    }
}
