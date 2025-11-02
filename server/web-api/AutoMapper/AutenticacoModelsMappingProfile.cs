using AutoMapper;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.WebAPI.Models.ModuloAutenticacao;

namespace GestaoDeEstacionamento.WebAPI.AutoMapper;

public class AutenticacaoModelsMappingProfile : Profile
{
    public AutenticacaoModelsMappingProfile()
    {
        CreateMap<(RegistrarUsuarioRequest r, ITenantProvider t), RegistrarUsuarioCommand>()
            .ConvertUsing(src => new RegistrarUsuarioCommand(
                src.r.NomeCompleto,
                src.r.Email,
                src.r.Senha,
                src.r.ConfirmarSenha,
                src.t.TenantId.HasValue ? src.t.TenantId : Guid.Empty,
                src.t.Slug
            ));
        CreateMap<(AutenticarUsuarioRequest a, ITenantProvider t), AutenticarUsuarioCommand>()
            .ConvertUsing(src => new AutenticarUsuarioCommand(
                src.a.Email,
                src.a.Senha,
                src.t.TenantId.HasValue ? src.t.TenantId : Guid.Empty,
                src.t.Slug
            ));
    }
}
