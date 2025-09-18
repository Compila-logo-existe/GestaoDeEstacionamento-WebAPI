using AutoMapper;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Core.Aplicacao.AutoMapper;

public class EstacionamentoMappingProfile : Profile
{
    public EstacionamentoMappingProfile()
    {
        #region ConfigurarEstacionamento
        CreateMap<ConfigurarEstacionamentoCommand, Estacionamento>();
        CreateMap<Estacionamento, ConfigurarEstacionamentoResult>()
            .ConvertUsing((src, dest, ctx) =>
                new ConfigurarEstacionamentoResult(
                    src.Id,
                    src.Nome,
                    QuantidadeDeVagasCriadas: src.Vagas.Count,
                    src?.Vagas.Select(c => ctx.Mapper.Map<ZonaEstacionamentoDto>(c)).ToImmutableList() ??
                    ImmutableList<ZonaEstacionamentoDto>.Empty
                )
            );

        CreateMap<Vaga, ZonaEstacionamentoDto>()
            .ConvertUsing(src =>
                new ZonaEstacionamentoDto(
                    src.Zona.ToString(),
                    src.Numero
                )
            );

        CreateMap<IEnumerable<Vaga>, ObterStatusVagasResult>()
            .ConvertUsing((src, dest, ctx) =>
                new ObterStatusVagasResult(
                    src?.Select(c => ctx.Mapper.Map<ObterStatusVagasDto>(c)).ToImmutableList() ??
                    ImmutableList<ObterStatusVagasDto>.Empty
                )
            );
        #endregion

        #region ObterStatusVagas
        CreateMap<Vaga, ObterStatusVagasDto>()
           .ConvertUsing(src =>
               new ObterStatusVagasDto(
                    src.Id,
                    src.Numero,
                    src.Zona,
                    src.Status,
                    src.Veiculo == null ? string.Empty : src.Veiculo.Placa
                )
           );

        CreateMap<IEnumerable<Vaga>, ObterStatusVagasResult>()
            .ConvertUsing((src, dest, ctx) =>
                new ObterStatusVagasResult(
                    src.Select(c => ctx.Mapper.Map<ObterStatusVagasDto>(c)).ToImmutableList() ??
                    ImmutableList<ObterStatusVagasDto>.Empty
                )
            );
        #endregion

        #region ObterVagaPorId
        CreateMap<Vaga, ObterVagaPorIdResult>()
           .ConvertUsing(src => new(new ObterStatusVagasDto(
                    src.Id,
                    src.Numero,
                    src.Zona,
                    src.Status,
                    src.Veiculo == null ? string.Empty : src.Veiculo.Placa
                ))
           );
        #endregion

        #region OcuparVaga
        CreateMap<(Estacionamento, Vaga), OcuparVagaResult>()
            .ConvertUsing(src => new OcuparVagaResult(
                new OcuparVagaDto(
                     true,
                    src.Item1.Nome,
                    $"{src.Item2.Zona}-{src.Item2.Numero}"
                )
            ));
        #endregion

        #region LiberarVaga
        CreateMap<(Estacionamento, Vaga), LiberarVagaResult>()
            .ConvertUsing(src => new LiberarVagaResult(
                new LiberarVagaDto(
                    true,
                    src.Item1.Nome,
                    $"{src.Item2.Zona}-{src.Item2.Numero}"
                )
            ));
        #endregion
    }
}
