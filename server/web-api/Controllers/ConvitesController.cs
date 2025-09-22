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
public class ConvitesController(IMediator mediator) : ControllerBase
{
    [HttpPost("tenants/{tenantId:guid}/convites")]
    [Authorize(Policy = "AdminOrPlatformAdminPolicy")]
    public async Task<IActionResult> CriarConvite(Guid tenantId, [FromBody] CriarConviteRequest req)
    {
        Result<(string Token, DateTime Expira)> r =
            await mediator.Send(new CriarConviteCommand(tenantId, req.EmailConvidado, req.NomeCargo));

        return r.IsSuccess ? Ok(new { tokenConvite = r.Value.Token, expiraEm = r.Value.Expira }) : BadRequest(r.Errors);
    }

    [HttpPost("convites/{token}/aceitar")]
    [AllowAnonymous]
    public async Task<IActionResult> Aceitar(string token, [FromBody] AceitarConviteRequest req)
    {
        Result<AccessToken> result = await mediator.Send(new AceitarConviteCommand(token, req.NomeCompleto, req.Senha, req.ConfirmarSenha));

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        return Ok(result.Value);
    }
}
