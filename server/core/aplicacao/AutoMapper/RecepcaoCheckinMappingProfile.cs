using AutoMapper;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using System.Collections.Immutable;

namespace GestaoDeEstacionamento.Core.Aplicacao.AutoMapper;

public class RecepcaoCheckinMappingProfile : Profile
{
    public RecepcaoCheckinMappingProfile()
    {
        #region Registrar
        CreateMap<RegistrarEntradaCommand, Hospede>();
        CreateMap<RegistrarEntradaCommand, Veiculo>();
        CreateMap<RegistrarEntradaCommand, RegistroEntrada>();
        CreateMap<RegistroEntrada, RegistrarEntradaResult>()
            .ConvertUsing(src => new RegistrarEntradaResult(
             src.Id,
             src.Ticket.NumeroSequencial
            ));
        #endregion

        #region ObterDetalhes
        CreateMap<Veiculo, ObterDetalhesVeiculoResult>()
            .ConvertUsing(src => new ObterDetalhesVeiculoResult(
                src.Id,
                src.Placa,
                src.Modelo,
                src.Cor,
                src.Observacoes,
                src.Hospede.NomeCompleto
            ));
        #endregion

        #region SeleçãoTodosRegistros
        CreateMap<RegistroEntrada, SelecionarRegistrosEntradaDto>()
           .ConvertUsing(src => new SelecionarRegistrosEntradaDto(
                src.Id,
                src.DataEntradaEmUtc,
                src.Observacoes,
                src.Hospede.Id,
                src.Hospede.NomeCompleto,
                src.Veiculo.Id,
                src.Veiculo.Placa
            ));

        CreateMap<IEnumerable<RegistroEntrada>, SelecionarRegistrosEntradaResult>()
         .ConvertUsing((src, dest, ctx) =>
             new SelecionarRegistrosEntradaResult(
                 src?.Select(c => ctx.Mapper.Map<SelecionarRegistrosEntradaDto>(c)).ToImmutableList() ??
                 ImmutableList<SelecionarRegistrosEntradaDto>.Empty
             )
         );
        #endregion

        #region SeleçãoTodosRegistrosDoVeiculo
        CreateMap<RegistroEntrada, SelecionarRegistrosEntradaDto>()
           .ConvertUsing(src => new SelecionarRegistrosEntradaDto(
                src.Id,
                src.DataEntradaEmUtc,
                src.Observacoes,
                src.Hospede.Id,
                src.Hospede.NomeCompleto,
                src.Veiculo.Id,
                src.Veiculo.Placa
            ));

        CreateMap<IEnumerable<RegistroEntrada>, SelecionarRegistrosDoVeiculoResult>()
         .ConvertUsing((src, dest, ctx) =>
             new SelecionarRegistrosDoVeiculoResult(
                 src?.Select(c => ctx.Mapper.Map<SelecionarRegistrosEntradaDto>(c)).ToImmutableList() ??
                 ImmutableList<SelecionarRegistrosEntradaDto>.Empty
             )
         );
        #endregion
    }
}
