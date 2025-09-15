using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Handlers;

public class ConfigurarEstacionamentoCommandHandler(
    IValidator<ConfigurarEstacionamentoCommand> validator,
    IMapper mapper,
    IRepositorioEstacionamento repositorioEstacionamento,
    ITenantProvider tenantProvider,
    IDistributedCache cache,
    IUnitOfWork unitOfWork,
    ILogger<ConfigurarEstacionamentoCommandHandler> logger
) : IRequestHandler<ConfigurarEstacionamentoCommand, Result<ConfigurarEstacionamentoResult>>
{
    public async Task<Result<ConfigurarEstacionamentoResult>> Handle(
        ConfigurarEstacionamentoCommand command, CancellationToken cancellationToken)
    {
        Guid? usuarioId = tenantProvider.UsuarioId;
        if (usuarioId is null || usuarioId == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário não identificado no tenant."));

        ValidationResult resultValidation = await validator.ValidateAsync(command, cancellationToken);

        if (!resultValidation.IsValid)
        {
            IEnumerable<string> erros = resultValidation.Errors.Select(e => e.ErrorMessage);
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro(erros));
        }

        try
        {
            Estacionamento novoEstacionamento = mapper.Map<Estacionamento>(command);
            novoEstacionamento.AderirUsuario(usuarioId.Value);

            await repositorioEstacionamento.CadastrarRegistroAsync(novoEstacionamento);

            await unitOfWork.CommitAsync();

            // Remove o cache de compromissos do usuário
            string cacheKey = $"contatos:u={tenantProvider.UsuarioId.GetValueOrDefault()}:q=all";

            await cache.RemoveAsync(cacheKey, cancellationToken);

            ConfigurarEstacionamentoResult result = mapper.Map<ConfigurarEstacionamentoResult>(novoEstacionamento);

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();

            logger.LogError(
                ex,
                "Ocorreu um erro durante o registro de {@Registro}.",
                command
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
        throw new NotImplementedException();
    }
}
