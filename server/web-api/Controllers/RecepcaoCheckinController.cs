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
    [HttpPost("registrar")]
    public async Task<ActionResult<RegistrarEntradaResponse>> RegistrarEntrada(RegistrarEntradaRequest request)
    {
        RegistrarEntradaCommand command = mapper.Map<RegistrarEntradaCommand>(request);

        Result<RegistrarEntradaResult> result = await mediator.Send(command);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        RegistrarEntradaResponse response = mapper.Map<RegistrarEntradaResponse>(result.Value);

        return Created(string.Empty, response);
    }

    [HttpGet("registros")]
    public async Task<ActionResult<SelecionarRegistrosEntradaResponse>> SelecionarRegistros(
        [FromQuery] SelecionarRegistrosEntradaRequest? request,
        CancellationToken ct
    )
    {
        SelecionarRegistrosEntradaQuery query = mapper.Map<SelecionarRegistrosEntradaQuery>(request);

        Result<SelecionarRegistrosEntradaResult> result = await mediator.Send(query, ct);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        SelecionarRegistrosEntradaResponse response = mapper.Map<SelecionarRegistrosEntradaResponse>(result.Value);

        return Ok(response);
    }

    [HttpGet("detalhes-veiculo/")]
    public async Task<ActionResult<ObterDetalhesVeiculoResponse>> ObterDetalhesVeiculo(
        [FromQuery] ObterDetalhesVeiculoRequest request,
        CancellationToken ct
    )
    {
        ObterDetalhesVeiculoQuery query = mapper.Map<ObterDetalhesVeiculoQuery>(request);

        Result<ObterDetalhesVeiculoResult> result = await mediator.Send(query, ct);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        ObterDetalhesVeiculoResponse response = mapper.Map<ObterDetalhesVeiculoResponse>(result.Value);

        return Ok(response);
    }

    [HttpGet("registros-veiculo/")]
    public async Task<ActionResult<SelecionarRegistrosDoVeiculoResponse>> SelecionarRegistrosDoVeiculo(
        [FromQuery] SelecionarRegistrosDoVeiculoRequest request,
        CancellationToken ct
    )
    {
        SelecionarRegistrosDoVeiculoQuery query = mapper.Map<SelecionarRegistrosDoVeiculoQuery>(request);

        Result<SelecionarRegistrosDoVeiculoResult> result = await mediator.Send(query, ct);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        SelecionarRegistrosDoVeiculoResponse response = mapper.Map<SelecionarRegistrosDoVeiculoResponse>(result.Value);

        return Ok(response);
    }
}
