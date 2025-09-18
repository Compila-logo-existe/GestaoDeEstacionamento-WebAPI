using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloFaturamento;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Handlers;

public class GerarRelatorioFinanceiroQueryHandler(
    IValidator<GerarRelatorioFinanceiroQuery> validator,
    IMapper mapper,
    IRepositorioFaturamento repositorioFaturamento,
    ITenantProvider tenantProvider,
    ILogger<GerarRelatorioFinanceiroQueryHandler> logger
) : IRequestHandler<GerarRelatorioFinanceiroQuery, Result<GerarRelatorioFinanceiroResult>>
{
    public async Task<Result<GerarRelatorioFinanceiroResult>> Handle(
        GerarRelatorioFinanceiroQuery query, CancellationToken cancellationToken)
    {
        Guid? usuarioId = tenantProvider.UsuarioId;
        if (usuarioId is null || usuarioId == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário não identificado no tenant."));

        ValidationResult resultValidation = await validator.ValidateAsync(query, cancellationToken);

        if (!resultValidation.IsValid)
        {
            IEnumerable<string> erros = resultValidation.Errors.Select(e => e.ErrorMessage);
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro(erros));
        }

        try
        {
            List<Faturamento> faturamentos = await repositorioFaturamento
                .SelecionarPorPeriodoAsync(query.DataInicial, query.DataFinal, usuarioId.Value, cancellationToken);

            if (faturamentos.Count == 0)
                return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro("Nenhum faturamento encontrado no período especificado."));

            decimal valorTotalPeriodo = faturamentos.Sum(i => i.ValorTotal);

            GerarRelatorioFinanceiroResult result = mapper.Map<GerarRelatorioFinanceiroResult>((query, faturamentos, valorTotalPeriodo));

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Ocorreu um erro ao gerar o relatório financeiro: {@Query}.",
                query
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }
}
