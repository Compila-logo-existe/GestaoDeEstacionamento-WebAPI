using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Handlers;

public class ObterVagaPorIdQueryHandler(
    IValidator<ObterVagaPorIdQuery> validator,
    IMapper mapper,
    IRepositorioVaga repositorioVaga,
    ITenantProvider tenantProvider,
    ILogger<ObterVagaPorIdQueryHandler> logger
) : IRequestHandler<ObterVagaPorIdQuery, Result<ObterVagaPorIdResult>>
{
    public async Task<Result<ObterVagaPorIdResult>> Handle(ObterVagaPorIdQuery query, CancellationToken cancellationToken)
    {
        Guid? usuarioId = tenantProvider.UsuarioId;
        if (usuarioId is null || usuarioId == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário não identificado no tenant."));

        ValidationResult valid = await validator.ValidateAsync(query, cancellationToken);

        if (!valid.IsValid)
        {
            IEnumerable<string> erros = valid.Errors.Select(e => e.ErrorMessage);
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro(erros));
        }

        try
        {
            Vaga? vaga = await repositorioVaga.SelecionarRegistroPorIdAsync(query.VagaId);

            if (vaga is null)
                return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro(query.VagaId));

            ObterStatusVagasDto dto = mapper.Map<ObterStatusVagasDto>(vaga);

            return Result.Ok(new ObterVagaPorIdResult(dto));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao obter vaga por Id. {@Query}", query);
            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }
}
