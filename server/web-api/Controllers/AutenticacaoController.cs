using AutoMapper;
using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.WebAPI.Helpers;
using GestaoDeEstacionamento.WebAPI.Models.ModuloAutenticacao;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoDeEstacionamento.WebAPI.Controllers;

[ApiController]
[Route("auth")]
public class AutenticacaoController(IMediator mediator, IMapper mapper) : Controller
{
    [HttpPost("registrar")]
    [AllowAnonymous]
    public async Task<ActionResult<AccessToken>> Registrar(
        [FromBody] RegistrarUsuarioRequest request,
        [FromServices] ITenantProvider tenantProvider)
    {
        RegistrarUsuarioCommand command = mapper.Map<RegistrarUsuarioCommand>((request, tenantProvider));

        Result<AccessToken> result = await mediator.Send(command);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        return Ok(result.Value);
    }

    [HttpPost("autenticar")]
    [AllowAnonymous]
    public async Task<ActionResult<AccessToken>> Autenticar(
        [FromBody] AutenticarUsuarioRequest request,
        [FromServices] ITenantProvider tenantProvider)
    {
        AutenticarUsuarioCommand command = mapper.Map<AutenticarUsuarioCommand>((request, tenantProvider));

        Result<AccessToken> result = await mediator.Send(command);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        return Ok(result.Value);
    }

    [HttpPost("sair")]
    [Authorize]
    public async Task<IActionResult> Sair()
    {
        Result result = await mediator.Send(new SairCommand());

        if (result.IsFailed)
            return this.MapearFalha(result);

        return NoContent();
    }
}
