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
    }
}
