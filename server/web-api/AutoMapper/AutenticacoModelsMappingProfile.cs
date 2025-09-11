using AutoMapper;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.WebAPI.Models.ModuloAutenticacao;

namespace GestaoDeEstacionamento.WebAPI.AutoMapper;

public class AutenticacaoModelsMappingProfile : Profile
{
    public AutenticacaoModelsMappingProfile()
    {
        CreateMap<RegistrarUsuarioRequest, RegistrarUsuarioCommand>();
        CreateMap<AutenticarUsuarioRequest, AutenticarUsuarioCommand>();
    }
}
