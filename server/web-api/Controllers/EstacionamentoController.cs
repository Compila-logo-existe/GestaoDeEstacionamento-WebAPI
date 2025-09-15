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
    [HttpPost("configuracao")]
    public async Task<ActionResult<ConfigurarEstacionamentoResponse>> RegistrarEntrada(ConfigurarEstacionamentoRequest request)
    {
        ConfigurarEstacionamentoCommand command = mapper.Map<ConfigurarEstacionamentoCommand>(request);

        Result<ConfigurarEstacionamentoResult> result = await mediator.Send(command);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        ConfigurarEstacionamentoResponse response = mapper.Map<ConfigurarEstacionamentoResponse>(result.Value);

        return Created(string.Empty, response);
    }
}
