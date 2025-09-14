using AutoMapper;
using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Handlers;
public class ObterDetalhesVeiculoPorIdQueryHandler(
    IMapper mapper,
    IRepositorioVeiculo repositorioVeiculo,
    ILogger<ObterDetalhesVeiculoPorIdQueryHandler> logger
) : IRequestHandler<ObterDetalhesVeiculoPorIdQuery, Result<ObterDetalhesVeiculoPorIdResult>>
{
    public async Task<Result<ObterDetalhesVeiculoPorIdResult>> Handle(
        ObterDetalhesVeiculoPorIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            Veiculo? veiculo = await repositorioVeiculo.SelecionarRegistroPorIdAsync(query.Id);

            if (veiculo is null)
                return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro(query.Id));

            ObterDetalhesVeiculoPorIdResult result = mapper.Map<ObterDetalhesVeiculoPorIdResult>(veiculo);

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Ocorreu um erro durante a seleção de {@Registro}.",
                query
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }
}
