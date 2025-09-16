using AutoMapper;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using GestaoDeEstacionamento.WebAPI.Models.ModuloEstacionamento;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.WebAPI.AutoMapper;

public class EstacionamentoModelsMappingProfile : Profile
{
    public EstacionamentoModelsMappingProfile()
    {
        #region Configurar
        CreateMap<ConfigurarEstacionamentoRequest, ConfigurarEstacionamentoCommand>();
        CreateMap<ConfigurarEstacionamentoCommand, ZonaEstacionamentoDto>();
        CreateMap<ConfigurarEstacionamentoResult, ConfigurarEstacionamentoResponse>()
            .ConvertUsing((src, dest, ctx) =>
            new ConfigurarEstacionamentoResponse(
                src.Id,
                src.Nome,
                src.QuantidadeDeVagasCriadas,
                src.Zonas
                )
            );
        #endregion

        #region ObterStatusVagas
        CreateMap<ObterStatusVagasRequest, ObterStatusVagasQuery>();
        CreateMap<ObterStatusVagasResult, ObterStatusVagasResponse>()
            .ConvertUsing((src, dest, ctx) =>
            new ObterStatusVagasResponse(
                src.Vagas.Count,
                src?.Vagas.Select(c => ctx.Mapper.Map<ObterStatusVagasDto>(c)).ToImmutableList() ??
                ImmutableList<ObterStatusVagasDto>.Empty
                )
            );
        #endregion
    }
}
