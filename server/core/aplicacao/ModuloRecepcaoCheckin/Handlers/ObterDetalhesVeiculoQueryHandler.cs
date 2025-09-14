using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Handlers;

public class ObterDetalhesVeiculoQueryHandler(
    IValidator<ObterDetalhesVeiculoQuery> validator,
    IMapper mapper,
    IRepositorioVeiculo repositorioVeiculo,
    ITenantProvider tenantProvider,
    ILogger<ObterDetalhesVeiculoQueryHandler> logger
) : IRequestHandler<ObterDetalhesVeiculoQuery, Result<ObterDetalhesVeiculoResult>>
{
    public async Task<Result<ObterDetalhesVeiculoResult>> Handle(
        ObterDetalhesVeiculoQuery query, CancellationToken cancellationToken)
    {
        try
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

            Veiculo? veiculoSelecionado = null!;

            if (query.VeiculoId.HasValue && query.VeiculoId.Value != Guid.Empty)
            {
                veiculoSelecionado = await repositorioVeiculo.SelecionarRegistroPorIdAsync(query.VeiculoId.Value);
                if (veiculoSelecionado is null)
                    return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro($"Veículo não encontrado. Id: {query.VeiculoId}"));
            }
            else if (!string.IsNullOrWhiteSpace(query.Placa))
            {
                query = query with
                {
                    Placa = Padronizador.PadronizarPlaca(query.Placa)
                };

                veiculoSelecionado = await repositorioVeiculo.SelecionarRegistroPorPlacaAsync(query.Placa, usuarioId, cancellationToken);
                if (veiculoSelecionado is null)
                    return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro($"Veículo não encontrado. Placa: {query.Placa}"));
            }

            ObterDetalhesVeiculoResult result = mapper.Map<ObterDetalhesVeiculoResult>(veiculoSelecionado);

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
