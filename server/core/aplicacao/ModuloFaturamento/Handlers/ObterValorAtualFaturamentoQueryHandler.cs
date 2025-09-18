using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Handlers;

public class ObterValorAtualFaturamentoQueryHandler(
    IValidator<ObterValorAtualFaturamentoQuery> validator,
    IMapper mapper,
    IRepositorioRegistroEntrada repositorioRegistroEntrada,
    IRepositorioEstacionamento repositorioEstacionamento,
    IRepositorioVaga repositorioVaga,
    ITenantProvider tenantProvider,
    ILogger<ObterValorAtualFaturamentoQueryHandler> logger
) : IRequestHandler<ObterValorAtualFaturamentoQuery, Result<ObterValorAtualFaturamentoResult>>
{
    public async Task<Result<ObterValorAtualFaturamentoResult>> Handle(
        ObterValorAtualFaturamentoQuery query, CancellationToken cancellationToken)
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
            RegistroEntrada? registroEntrada = query.NumeroSequencialDoTicket.HasValue
                ? await repositorioRegistroEntrada.SelecionarAberturaPorNumeroDoTicketAsync(
                    query.NumeroSequencialDoTicket.Value, usuarioId, cancellationToken)
                : await repositorioRegistroEntrada.SelecionarAberturaPorPlacaAsync(
                    query.Placa!, usuarioId, cancellationToken);

            if (registroEntrada is null)
                return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro("Registro de entrada não encontrado ou já encerrado."));

            Estacionamento? estacionamento = query.EstacionamentoId.HasValue
                ? await repositorioEstacionamento.SelecionarRegistroPorIdAsync(query.EstacionamentoId.Value)
                : await repositorioEstacionamento.SelecionarRegistroPorNome(
                    query.EstacionamentoNome!, usuarioId, cancellationToken);

            //if (!registroEntrada.Veiculo.Vaga.Estacionamento.Equals(estacionamento))
            //    return Result.Fail(ResultadosErro.ConflitoErro("A vaga selecionada não pertence ao estacionamento escolhido."));

            registroEntrada.Faturamento.RecalcularTotais();
            int numeroDeDiarias = registroEntrada.Faturamento.NumeroDeDiarias;
            decimal valorDaDiaria = registroEntrada.Faturamento.ValorDaDiaria;
            decimal valorTotalAtual = numeroDeDiarias * valorDaDiaria;

            ObterValorAtualFaturamentoResult result = mapper.Map<ObterValorAtualFaturamentoResult>((numeroDeDiarias, valorDaDiaria, valorTotalAtual));

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Ocorreu um erro: {@Query}.",
                query
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }
}
