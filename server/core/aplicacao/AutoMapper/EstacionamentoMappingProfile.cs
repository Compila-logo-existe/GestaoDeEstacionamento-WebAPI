using AutoMapper;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;

namespace GestaoDeEstacionamento.Core.Aplicacao.AutoMapper;

public class EstacionamentoMappingProfile : Profile
{
    public EstacionamentoMappingProfile()
    {
        #region Configurar
        CreateMap<ConfigurarEstacionamentoCommand, Estacionamento>();
        CreateMap<Estacionamento, ConfigurarEstacionamentoResult>();
        #endregion
    }
}
