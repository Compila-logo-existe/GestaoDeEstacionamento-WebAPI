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

public class OcuparVagaCommandHandler(
    IValidator<OcuparVagaCommand> validator,
    IMapper mapper,
    IRepositorioEstacionamento repositorioEstacionamento,
    IRepositorioRegistroEntrada repositorioRegistroEntrada,
    IRepositorioVaga repositorioVaga,
    IRepositorioVeiculo repositorioVeiculo,
    ITenantProvider tenantProvider,
    IDistributedCache cache,
    IUnitOfWork unitOfWork,
    ILogger<OcuparVagaCommandHandler> logger
) : IRequestHandler<OcuparVagaCommand, Result<OcuparVagaResult>>
{
    public async Task<Result<OcuparVagaResult>> Handle(
        OcuparVagaCommand command, CancellationToken cancellationToken)
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

            Veiculo? veiculoSelecionado = null!;
            if (command.VeiculoId.HasValue && command.VeiculoId.Value != Guid.Empty)
            {
                veiculoSelecionado = await repositorioVeiculo.SelecionarRegistroPorIdAsync(command.VeiculoId.Value);
                if (veiculoSelecionado is null)
                    return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro($"Veículo não encontrado. Id: {command.VeiculoId}"));
            }
            else if (!string.IsNullOrWhiteSpace(command.Placa))
            {
                command = command with
                {
                    Placa = Padronizador.PadronizarPlaca(command.Placa)
                };

                veiculoSelecionado = await repositorioVeiculo.SelecionarRegistroPorPlacaAsync(command.Placa, tenantId, cancellationToken);
                if (veiculoSelecionado is null)
                    return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro($"Veículo não encontrado. Placa: {command.Placa}"));
            }

            if (!vagaSelecionada.Estacionamento.Equals(estacionamentoSelecionado))
                return Result.Fail(ResultadosErro.ConflitoErro("A vaga escolhida não pertence a este estacionaento."));

            bool possuiEntradaEmAberto = await repositorioRegistroEntrada
                .ExisteAberturaPorPlacaAsync(veiculoSelecionado.Placa, tenantId, cancellationToken);
            if (!possuiEntradaEmAberto)
                return Result.Fail(ResultadosErro.ConflitoErro("Não existe check-in/ticket aberto para este veículo."));

            if (vagaSelecionada.EstaOcupada)
                return Result.Fail(ResultadosErro.ConflitoErro("A vaga já está ocupada."));

            vagaSelecionada.Ocupar(veiculoSelecionado);
            vagaSelecionada.VincularTenant(tenantId.Value);

            await unitOfWork.CommitAsync();

            await cache.RemoveAsync($"estacionamento:t={tenantId}:q=all:e={estacionamentoSelecionado.Id}:z=all", cancellationToken);
            if (!string.IsNullOrWhiteSpace(command.VagaZona))
                await cache.RemoveAsync($"estacionamento:t={tenantId}:q=all:e={estacionamentoSelecionado.Id}:z={command.VagaZona}", cancellationToken);

            await cache.RemoveAsync($"estacionamento:t={tenantId}:q=all:e={estacionamentoSelecionado.Nome}:z=all", cancellationToken);
            if (!string.IsNullOrWhiteSpace(command.VagaZona))
                await cache.RemoveAsync($"estacionamento:t={tenantId}:q=all:e={estacionamentoSelecionado.Nome}:z={command.VagaZona}", cancellationToken);

            OcuparVagaResult result = mapper.Map<(Estacionamento, Vaga), OcuparVagaResult>(
                (estacionamentoSelecionado, vagaSelecionada));

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Ocorreu um erro durante a ocupação da vaga {@Id}.",
                command.VagaId
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }
}
