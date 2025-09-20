using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Handlers;

public class LiberarVagaCommandHandler(
    IValidator<LiberarVagaCommand> validator,
    IMapper mapper,
    IRepositorioEstacionamento repositorioEstacionamento,
    IRepositorioRegistroEntrada repositorioRegistroEntrada,
    IRepositorioVaga repositorioVaga,
    ITenantProvider tenantProvider,
    IDistributedCache cache,
    IUnitOfWork unitOfWork,
    ILogger<LiberarVagaCommandHandler> logger
) : IRequestHandler<LiberarVagaCommand, Result<LiberarVagaResult>>
{
    public async Task<Result<LiberarVagaResult>> Handle(
        LiberarVagaCommand command, CancellationToken cancellationToken)
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

        try
        {
            Estacionamento? estacionamentoSelecionado = null!;
            if (command.EstacionamentoId.HasValue && command.EstacionamentoId.Value != Guid.Empty)
            {
                estacionamentoSelecionado = await repositorioEstacionamento.SelecionarRegistroPorIdAsync(command.EstacionamentoId.Value);
                if (estacionamentoSelecionado is null)
                    return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro($"Estacionamento não encontrado. Id: {command.EstacionamentoId.Value}"));
            }
            else if (!string.IsNullOrWhiteSpace(command.EstacionamentoNome))
            {
                estacionamentoSelecionado = await repositorioEstacionamento.SelecionarRegistroPorNome(command.EstacionamentoNome, tenantId, cancellationToken);
                if (estacionamentoSelecionado is null)
                    return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro($"Estacionamento não encontrado. Nome: {command.EstacionamentoNome}"));
            }

            Vaga? vagaSelecionada = null!;
            if (command.VagaId.HasValue)
            {
                vagaSelecionada = await repositorioVaga.SelecionarRegistroPorIdAsync(command.VagaId.Value);
                if (vagaSelecionada is null)
                    return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro($"Vaga não encontrada. Id: {command.VagaId.Value}"));
            }
            else if (command.VagaNumero.HasValue && !string.IsNullOrWhiteSpace(command.VagaZona))
            {

                ZonaEstacionamento? zona = null!;

                if (!string.IsNullOrWhiteSpace(command.VagaZona) &&
                    Enum.TryParse(command.VagaZona, true, out ZonaEstacionamento zonaConvertida))
                {
                    zona = zonaConvertida;
                }

                vagaSelecionada = await repositorioVaga.SelecionarRegistroPorDadosAsync(command.VagaNumero.Value, zona, estacionamentoSelecionado.Id, tenantId, cancellationToken);
                if (vagaSelecionada is null)
                    return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro($"Vaga não encontrada. Dados: {command.VagaZona}-{command.VagaNumero}"));
            }

            if (!string.IsNullOrWhiteSpace(command.Placa))
            {
                string placaPadronizada = Padronizador.PadronizarPlaca(command.Placa);
                bool possuiEntradaEmAberto = await repositorioRegistroEntrada
                    .ExisteAberturaPorPlacaAsync(placaPadronizada, tenantId, cancellationToken);

                if (!possuiEntradaEmAberto)
                    return Result.Fail(ResultadosErro.ConflitoErro("Não existe check-in/ticket aberto para esta placa."));
            }

            if (!vagaSelecionada.EstaOcupada)
                return Result.Fail(ResultadosErro.ConflitoErro("A vaga não está ocupada."));

            vagaSelecionada.Liberar();
            vagaSelecionada.VincularTenant(tenantId.Value);

            await unitOfWork.CommitAsync();

            await cache.RemoveAsync($"estacionamento:t={tenantId}:q=all:e={estacionamentoSelecionado.Id}:z=all", cancellationToken);
            if (!string.IsNullOrWhiteSpace(command.VagaZona))
                await cache.RemoveAsync($"estacionamento:t={tenantId}:q=all:e={estacionamentoSelecionado.Id}:z={command.VagaZona}", cancellationToken);

            await cache.RemoveAsync($"estacionamento:t={tenantId}:q=all:e={estacionamentoSelecionado.Nome}:z=all", cancellationToken);
            if (!string.IsNullOrWhiteSpace(command.VagaZona))
                await cache.RemoveAsync($"estacionamento:t={tenantId}:q=all:e={estacionamentoSelecionado.Nome}:z={command.VagaZona}", cancellationToken);

            LiberarVagaResult result = mapper.Map<(Estacionamento, Vaga), LiberarVagaResult>(
                (estacionamentoSelecionado, vagaSelecionada));

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Ocorreu um erro durante a liberação da vaga {@Id}.",
                command.VagaId
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }
}
