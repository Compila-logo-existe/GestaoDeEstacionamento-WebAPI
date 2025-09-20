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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Handlers;

public class ConfigurarEstacionamentoCommandHandler(
    IValidator<ConfigurarEstacionamentoCommand> validator,
    IMapper mapper,
    IRepositorioEstacionamento repositorioEstacionamento,
    IRepositorioVaga repositorioVaga,
    ITenantProvider tenantProvider,
    IDistributedCache cache,
    IUnitOfWork unitOfWork,
    ILogger<ConfigurarEstacionamentoCommandHandler> logger
) : IRequestHandler<ConfigurarEstacionamentoCommand, Result<ConfigurarEstacionamentoResult>>
{
    public async Task<Result<ConfigurarEstacionamentoResult>> Handle(
        ConfigurarEstacionamentoCommand command, CancellationToken cancellationToken)
    {
        Guid? tenantId = tenantProvider.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Tenant não informado. Envie o header 'X-Tenant-Id'."));

        Guid? usuarioId = tenantProvider.UsuarioId;
        if (!usuarioId.HasValue || usuarioId.Value == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário autenticado não identificado."));

        ValidationResult resultValidation = await validator.ValidateAsync(command, cancellationToken);

        if (!resultValidation.IsValid)
        {
            IEnumerable<string> erros = resultValidation.Errors.Select(e => e.ErrorMessage);
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro(erros));
        }

        Result<IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga>> tentativa = DistribuidorDeVagas.TentarGerarEsquemaDeVagas(
            command.QuantidadeVagas,
            command.ZonasTotais,
            command.VagasPorZona
        );

        if (tentativa.IsFailed)
        {
            IEnumerable<string> mensagens = tentativa.Errors.Select(e => e.Message);
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro(mensagens));
        }

        IReadOnlyList<DistribuidorDeVagas.PosicaoDaVaga> posicoes = tentativa.Value;

        try
        {
            Estacionamento novoEstacionamento = mapper.Map<Estacionamento>(command);
            novoEstacionamento.VincularTenant(tenantId.Value);

            await repositorioEstacionamento.CadastrarRegistroAsync(novoEstacionamento);

            foreach (DistribuidorDeVagas.PosicaoDaVaga p in posicoes)
            {
                Vaga vaga = new()
                {
                    EstacionamentoId = novoEstacionamento.Id,
                    Zona = p.Zona,
                    Numero = p.Numero
                };
                vaga.VincularTenant(tenantId.Value);

                await repositorioVaga.CadastrarRegistroAsync(vaga);
            }

            await unitOfWork.CommitAsync();

            await cache.RemoveAsync($"estacionamento:t={tenantId}:q=all:e=all:z=all", cancellationToken);

            foreach (ZonaEstacionamento zona in Enum.GetValues<ZonaEstacionamento>())
            {
                await cache.RemoveAsync($"estacionamento:t={tenantId}:q=all:e=all:z={zona}", cancellationToken);
            }

            await cache.RemoveAsync($"estacionamento:t={tenantId}:q=all:e={novoEstacionamento.Id}:z=all", cancellationToken);
            foreach (ZonaEstacionamento zona in Enum.GetValues<ZonaEstacionamento>())
            {
                await cache.RemoveAsync($"estacionamento:t={tenantId}:q=all:e={novoEstacionamento.Id}:z={zona}", cancellationToken);
            }

            await cache.RemoveAsync($"estacionamento:t={tenantId}:q=all:e={novoEstacionamento.Nome}:z=all", cancellationToken);
            foreach (ZonaEstacionamento zona in Enum.GetValues<ZonaEstacionamento>())
            {
                await cache.RemoveAsync($"estacionamento:t={tenantId}:q=all:e={novoEstacionamento.Nome}:z={zona}", cancellationToken);
            }

            foreach (ZonaEstacionamento zona in Enum.GetValues<ZonaEstacionamento>())
            {
                await cache.RemoveAsync($"estacionamento:t={tenantId}:q=all,z={zona}", cancellationToken);
            }

            ConfigurarEstacionamentoResult result = mapper.Map<ConfigurarEstacionamentoResult>(novoEstacionamento);

            return Result.Ok(result);
        }
        catch (DbUpdateException ex)
        {
            await unitOfWork.RollbackAsync();

            logger.LogError(
                ex,
                "Erro de persistência ao configurar estacionamento {@Command}.",
                command
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
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
