using AutoMapper;
using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Commands;
using GestaoDeEstacionamento.WebAPI.Helpers;
using GestaoDeEstacionamento.WebAPI.Models.ModuloFaturamento;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoDeEstacionamento.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("faturamento")]
public class FaturamentoController(
    IMapper mapper,
    IMediator mediator
) : ControllerBase
{
    [HttpGet("obter-valor-atual")]
    public async Task<ActionResult<ObterValorAtualFaturamentoResponse>> ObterValorAtual(
        [FromQuery] ObterValorAtualFaturamentoRequest request, CancellationToken ct)
    {
        ObterValorAtualFaturamentoQuery query = mapper.Map<ObterValorAtualFaturamentoQuery>(request);

        Result<ObterValorAtualFaturamentoResult> result = await mediator.Send(query, ct);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        ObterValorAtualFaturamentoResponse response = mapper.Map<ObterValorAtualFaturamentoResponse>(result.Value);

        return Ok(response);
    }

    [HttpGet("gerar-relatorio-financeiro")]
    public async Task<ActionResult<GerarRelatorioFinanceiroResponse>> GerarRelatorioFinanceiro(
        [FromQuery] GerarRelatorioFinanceiroRequest request, CancellationToken ct)
    {
        GerarRelatorioFinanceiroQuery query = mapper.Map<GerarRelatorioFinanceiroQuery>(request);

        Result<GerarRelatorioFinanceiroResult> result = await mediator.Send(query, ct);

        if (result.IsFailed)
            return this.MapearFalha(result.ToResult());

        GerarRelatorioFinanceiroResponse response = mapper.Map<GerarRelatorioFinanceiroResponse>(result.Value);

        return Ok(response);
    }
}
