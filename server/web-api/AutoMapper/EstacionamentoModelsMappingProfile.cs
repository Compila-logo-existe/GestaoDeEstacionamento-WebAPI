using AutoMapper;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using GestaoDeEstacionamento.WebAPI.Models.ModuloEstacionamento;

namespace GestaoDeEstacionamento.WebAPI.AutoMapper;

public class EstacionamentoModelsMappingProfile : Profile
{
    public EstacionamentoModelsMappingProfile()
    {
        #region Configurar
        CreateMap<ConfigurarEstacionamentoRequest, ConfigurarEstacionamentoCommand>();
        CreateMap<ConfigurarEstacionamentoResult, ConfigurarEstacionamentoResponse>();
        #endregion
    }
}
