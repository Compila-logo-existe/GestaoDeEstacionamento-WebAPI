using AutoMapper;
using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Handlers;
public class ObterDetalhesVeiculoPorPlacaQueryHandler(
    IMapper mapper,
    IRepositorioVeiculo repositorioVeiculo,
    ITenantProvider tenantProvider,
    ILogger<ObterDetalhesVeiculoPorPlacaQueryHandler> logger
) : IRequestHandler<ObterDetalhesVeiculoPorPlacaQuery, Result<ObterDetalhesVeiculoPorPlacaResult>>
{
    public async Task<Result<ObterDetalhesVeiculoPorPlacaResult>> Handle(
        ObterDetalhesVeiculoPorPlacaQuery query, CancellationToken cancellationToken)
    {
        try
        {
            Guid? usuarioId = tenantProvider.UsuarioId;
            if (usuarioId is null || usuarioId == Guid.Empty)
                return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário não identificado no tenant."));

            Veiculo? veiculo = await repositorioVeiculo.SelecionarRegistroPorPlacaAsync(query.Placa, usuarioId, cancellationToken);

            if (veiculo is null)
                return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro(query.Placa));

            ObterDetalhesVeiculoPorPlacaResult result = mapper.Map<ObterDetalhesVeiculoPorPlacaResult>(veiculo);

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
