using AutoMapper;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

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

        #region ObterDetalhesPorId
        CreateMap<Veiculo, ObterDetalhesVeiculoPorIdResult>()
            .ConvertUsing(src => new ObterDetalhesVeiculoPorIdResult(
                src.Id,
                src.Placa,
                src.Modelo,
                src.Cor,
                src.Observacoes,
                src.Hospede.NomeCompleto
            ));
        #endregion

        #region ObterDetalhesPorPlaca
        CreateMap<Veiculo, ObterDetalhesVeiculoPorPlacaResult>()
            .ConvertUsing(src => new ObterDetalhesVeiculoPorPlacaResult(
                src.Id,
                src.Placa,
                src.Modelo,
                src.Cor,
                src.Observacoes,
                src.Hospede.NomeCompleto
            ));
        #endregion
    }
}
