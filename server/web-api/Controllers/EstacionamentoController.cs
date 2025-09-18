using AutoMapper;
using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using GestaoDeEstacionamento.WebAPI.Helpers;
using GestaoDeEstacionamento.WebAPI.Models.ModuloEstacionamento;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoDeEstacionamento.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("estacionamento")]
public class EstacionamentoController(
    IMapper mapper,
    IMediator mediator
) : ControllerBase
{
    [HttpPost("configuracao/")]
    public async Task<ActionResult<ConfigurarEstacionamentoResponse>> ConfigurarEstacionamento(
        ConfigurarEstacionamentoRequest request,
        CancellationToken ct
    )
    {
        ConfigurarEstacionamentoCommand command = mapper.Map<ConfigurarEstacionamentoCommand>(request);

        Result<ConfigurarEstacionamentoResult> result = await mediator.Send(command, ct);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        ConfigurarEstacionamentoResponse response = mapper.Map<ConfigurarEstacionamentoResponse>(result.Value);

        return Created(string.Empty, response);
    }

    [HttpGet("status-vagas/")]
    public async Task<ActionResult<ObterStatusVagasResponse>> ObterStatusVagas(
        [FromQuery] ObterStatusVagasRequest request,
        CancellationToken ct
    )
    {
        ObterStatusVagasQuery query = mapper.Map<ObterStatusVagasQuery>(request);

        Result<ObterStatusVagasResult> result = await mediator.Send(query, ct);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        ObterStatusVagasResponse response = mapper.Map<ObterStatusVagasResponse>(result.Value);

        return Ok(response);
    }

    [HttpPost("ocupar-vaga/")]
    public async Task<ActionResult<OcuparVagaResponse>> OcuparVaga(
        [FromQuery] OcuparVagaRequest request,
        CancellationToken ct
    )
    {
        OcuparVagaCommand command = mapper.Map<OcuparVagaCommand>(request);

        Result<OcuparVagaResult> result = await mediator.Send(command, ct);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        OcuparVagaResponse response = mapper.Map<OcuparVagaResponse>(result.Value);

        return Ok(response);
    }

    [HttpPost("liberar-vaga/")]
    public async Task<ActionResult<LiberarVagaResponse>> LiberarVaga(
    [FromQuery] LiberarVagaRequest request,
    CancellationToken ct
    )
    {
        LiberarVagaCommand command = mapper.Map<LiberarVagaCommand>(request);

        Result<LiberarVagaResult> result = await mediator.Send(command, ct);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        LiberarVagaResponse response = mapper.Map<LiberarVagaResponse>(result.Value);

        return Ok(response);
    }
}