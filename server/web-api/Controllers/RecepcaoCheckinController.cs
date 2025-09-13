using AutoMapper;
using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.WebAPI.Models.ModuloRecepcaoCheckin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GestaoDeEstacionamento.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("recepcao")]
public class RecepcaoCheckinController(
    IMapper mapper,
    IMediator mediator
) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RegistrarEntradaResponse>> RegistrarEntrada(RegistrarEntradaRequest request)
    {
        RegistrarEntradaCommand command = mapper.Map<RegistrarEntradaCommand>(request);

        Result<RegistrarEntradaResult> result = await mediator.Send(command);

        if (result.IsFailed)
        {
            if (result.HasError(e => e.HasMetadataKey("TipoErro")))
            {
                IEnumerable<string> errosDeValidacao = result.Errors
                    .SelectMany(e => e.Reasons.OfType<IError>())
                    .Select(e => e.Message);

                return BadRequest(errosDeValidacao);
            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        RegistrarEntradaResponse response = mapper.Map<RegistrarEntradaResponse>(result.Value);

        return Created(string.Empty, response);
    }
}
