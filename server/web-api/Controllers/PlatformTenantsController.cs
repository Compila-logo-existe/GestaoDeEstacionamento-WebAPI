using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.WebAPI.Helpers;
using GestaoDeEstacionamento.WebAPI.Models.ModuloAutenticacao;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoDeEstacionamento.WebAPI.Controllers;

[ApiController]
[Route("platform/tenants")]
[Authorize(Policy = "PlatformAdminPolicy")]
public class PlatformTenantsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarTenantRequest request)
    {
        Result<Guid> result = await mediator.Send(new CriarTenantCommand(request.Nome, request.CNPJ, request.SlugSubdominio, request.DominioPersonalizado));

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        return Ok(new { tenantId = result.Value });
    }
}
