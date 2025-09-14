using AutoMapper;
using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.WebAPI.Helpers;
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
            return this.MapearFalha(result.ToResult());

        RegistrarEntradaResponse response = mapper.Map<RegistrarEntradaResponse>(result.Value);

        return Created(string.Empty, response);
    }

    [HttpGet]
    public async Task<ActionResult<SelecionarRegistrosEntradaResponse>> SelecionarRegistros(
        [FromQuery] SelecionarRegistrosEntradaRequest? request,
        CancellationToken cancellationToken
    )
    {
        SelecionarRegistrosEntradaQuery query = mapper.Map<SelecionarRegistrosEntradaQuery>(request);

        Result<SelecionarRegistrosEntradaResult> result = await mediator.Send(query, cancellationToken);

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

        SelecionarRegistrosEntradaResponse response = mapper.Map<SelecionarRegistrosEntradaResponse>(result.Value);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ObterDetalhesVeiculoPorIdResponse>> ObterDetalhesVeiculoPorId(Guid id)
    {
        ObterDetalhesVeiculoPorIdQuery query = mapper.Map<ObterDetalhesVeiculoPorIdQuery>(id);

        Result<ObterDetalhesVeiculoPorIdResult> result = await mediator.Send(query);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        ObterDetalhesVeiculoPorIdResponse response = mapper.Map<ObterDetalhesVeiculoPorIdResponse>(result.Value);

        return Ok(response);
    }

    [HttpGet("{placa}")]
    public async Task<ActionResult<ObterDetalhesVeiculoPorPlacaResponse>> ObterDetalhesVeiculoPorPlaca(string placa)
    {
        ObterDetalhesVeiculoPorPlacaQuery query = mapper.Map<ObterDetalhesVeiculoPorPlacaQuery>(placa);

        Result<ObterDetalhesVeiculoPorPlacaResult> result = await mediator.Send(query);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        ObterDetalhesVeiculoPorPlacaResponse response = mapper.Map<ObterDetalhesVeiculoPorPlacaResponse>(result.Value);

        return Ok(response);
    }
}
