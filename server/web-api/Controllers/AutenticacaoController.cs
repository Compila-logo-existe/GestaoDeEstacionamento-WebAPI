using AutoMapper;
using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.WebAPI.Helpers;
using GestaoDeEstacionamento.WebAPI.Models.ModuloAutenticacao;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GestaoDeEstacionamento.WebAPI.Controllers;

[ApiController]
[Route("auth")]
public class AutenticacaoController(IMediator mediator, IMapper mapper) : Controller
{
    [HttpPost("registrar")]
    [AllowAnonymous]
    public async Task<ActionResult<AccessToken>> Registrar(
    [FromBody] RegistrarUsuarioRequest request,
    [FromServices] ITenantProvider tenantProvider,
    [FromServices] IRefreshTokenProvider refreshProvider,
    [FromServices] IRefreshTokenCookieService cookieService,
    [FromServices] IOptions<OpcoesRefreshToken> opcoes,
    [FromServices] UserManager<Usuario> userManager)
    {
        RegistrarUsuarioCommand command = mapper.Map<RegistrarUsuarioCommand>((request, tenantProvider));
        Result<AccessToken> result = await mediator.Send(command);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        Usuario? usuario = await userManager.FindByIdAsync(result.Value.UsuarioAutenticado.Id.ToString());
        if (usuario is not null)
        {
            Result<string> gerar = await refreshProvider.GerarRefreshTokenAsync(
                    result.Value.UsuarioAutenticado.Id,
                    tenantProvider.TenantId!.Value,
                    HttpContext.RequestAborted
            );

            if (gerar.IsSuccess)
            {
                bool crossSite = false;
                cookieService.Gravar(gerar.Value, opcoes.Value.QuantidadeDiasDeValidade, crossSite);
            }
        }

        return Ok(result.Value);
    }

    [HttpPost("autenticar")]
    [AllowAnonymous]
    public async Task<ActionResult<AccessToken>> Autenticar(
    [FromBody] AutenticarUsuarioRequest request,
    [FromServices] ITenantProvider tenantProvider,
    [FromServices] IRefreshTokenProvider refreshProvider,
    [FromServices] IRefreshTokenCookieService cookieService,
    [FromServices] IOptions<OpcoesRefreshToken> opcoes,
    [FromServices] UserManager<Usuario> userManager)
    {
        AutenticarUsuarioCommand command = mapper.Map<AutenticarUsuarioCommand>((request, tenantProvider));
        Result<AccessToken> result = await mediator.Send(command);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        Usuario? usuario = await userManager.FindByIdAsync(result.Value.UsuarioAutenticado.Id.ToString());
        if (usuario is not null)
        {
            Result<string> gerar = await refreshProvider.GerarRefreshTokenAsync(
                    result.Value.UsuarioAutenticado.Id,
                    tenantProvider.TenantId!.Value,
                    HttpContext.RequestAborted
            );

            if (gerar.IsSuccess)
            {
                const bool crossSite = false;
                cookieService.Gravar(gerar.Value, opcoes.Value.QuantidadeDiasDeValidade, crossSite);
            }
        }

        return Ok(result.Value);
    }


    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AccessToken>> Refresh(
    [FromServices] IRefreshTokenCookieService cookieService,
    [FromServices] IOptions<OpcoesRefreshToken> opcoesAccessor)
    {
        string? valorDoCookie = cookieService.Ler();
        if (string.IsNullOrWhiteSpace(valorDoCookie))
            return Unauthorized(new { mensagem = "Refresh token ausente." });

        RotacionarTokenCommand comando = new(valorDoCookie);
        Result<(AccessToken AccessToken, string RefreshToken)> result = await mediator.Send(comando);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        cookieService.Remover();
        const bool ambienteCrossSite = false;
        cookieService.Gravar(result.Value.RefreshToken, opcoesAccessor.Value.QuantidadeDiasDeValidade, ambienteCrossSite);

        return Ok(result.Value.AccessToken);
    }


    [HttpPost("sair")]
    [Authorize]
    public async Task<IActionResult> Sair([FromServices] IRefreshTokenCookieService cookieService)
    {
        Result result = await mediator.Send(new SairCommand());

        if (result.IsFailed)
            return this.MapearFalha(result);

        cookieService.Remover();

        return Ok(new { loggedOut = true });
    }
}
