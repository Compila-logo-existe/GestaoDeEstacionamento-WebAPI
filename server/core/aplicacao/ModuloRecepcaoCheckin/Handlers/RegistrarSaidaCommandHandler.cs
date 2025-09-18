using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Handlers;

public class RegistrarSaidaCommandHandler(
    IValidator<RegistrarSaidaCommand> validator,
    IMapper mapper,
    IRepositorioHospede repositorioHospede,
    IRepositorioVeiculo repositorioVeiculo,
    IRepositorioRegistroEntrada repositorioRegistroEntrada,
    IRepositorioRegistroSaida RepositorioRegistroSaida,
    ITenantProvider tenantProvider,
    IDistributedCache cache,
    IUnitOfWork unitOfWork,
    ILogger<RegistrarSaidaCommandHandler> logger
) : IRequestHandler<RegistrarSaidaCommand, Result<RegistrarSaidaResult>>
{
    public async Task<Result<RegistrarSaidaResult>> Handle(
        RegistrarSaidaCommand command, CancellationToken cancellationToken)
    {
        Guid? usuarioId = tenantProvider.UsuarioId;
        if (usuarioId is null || usuarioId == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário não identificado no tenant."));

        command = command with
        {
            CPF = Padronizador.PadronizarCPF(command.CPF),
            Placa = Padronizador.PadronizarPlaca(command.Placa)
        };

        ValidationResult resultValidation = await validator.ValidateAsync(command, cancellationToken);

        if (!resultValidation.IsValid)
        {
            IEnumerable<string> erros = resultValidation.Errors.Select(e => e.ErrorMessage);
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro(erros));
        }

        try
        {
            Hospede? hospedeSelecionado = (command.HospedeId.HasValue && command.HospedeId.Value != Guid.Empty)
                ? await repositorioHospede.SelecionarRegistroPorIdAsync(command.HospedeId.Value)
                : await repositorioHospede.SelecionarRegistroPorCPFAsync(command.CPF, usuarioId, cancellationToken);

            if (hospedeSelecionado is null)
                return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro("Hóspede não encontrado."));

            Veiculo? veiculoSelecionado = (command.VeiculoId.HasValue && command.VeiculoId.Value != Guid.Empty)
                ? await repositorioVeiculo.SelecionarRegistroPorIdAsync(command.VeiculoId.Value)
                : await repositorioVeiculo.SelecionarRegistroPorPlacaAsync(command.Placa, usuarioId, cancellationToken);

            if (veiculoSelecionado is null)
                return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro("Veículo não encontrado."));

            hospedeSelecionado.AderirVeiculo(veiculoSelecionado);

            bool possuiEntradaEmAberto = await repositorioRegistroEntrada.ExisteAberturaPorPlacaAsync(command.Placa, usuarioId, cancellationToken);
            if (!possuiEntradaEmAberto)
                return Result.Fail(ResultadosErro.ConflitoErro("Não existe check-in/ticket em aberto para esta placa."));

            RegistroEntrada? registroEntrada = await repositorioRegistroEntrada.SelecionarAberturaPorNumeroDoTicketAsync(command.NumeroSequencialDoTicket, usuarioId, cancellationToken)
                                ?? await repositorioRegistroEntrada.SelecionarAberturaPorPlacaAsync(command.Placa!, usuarioId, cancellationToken);

            if (registroEntrada is null)
                return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro("Registro de entrada não encontrado ou já encerrado."));

            Guid? estacionamentoId = veiculoSelecionado.Vaga?.Estacionamento?.Id ?? Guid.Empty;
            string? estacionamentoNome = veiculoSelecionado.Vaga?.Estacionamento?.Nome ?? string.Empty;
            string? vagaZona = veiculoSelecionado.Vaga?.Zona.ToString() ?? string.Empty;

            veiculoSelecionado.Vaga?.Liberar();

            RegistroSaida novoRegistroSaida = mapper.Map<RegistroSaida>(command);
            novoRegistroSaida.AderirUsuario(usuarioId.Value);
            novoRegistroSaida.AderirHospede(hospedeSelecionado);
            novoRegistroSaida.AderirVeiculo(veiculoSelecionado);
            novoRegistroSaida.AderirTicket(registroEntrada.Ticket);

            registroEntrada.Faturamento.RegistrarSaida(novoRegistroSaida);
            registroEntrada.AderirDataSaida(novoRegistroSaida.DataSaidaEmUtc);

            await RepositorioRegistroSaida.CadastrarRegistroAsync(novoRegistroSaida);

            await unitOfWork.CommitAsync();

            await cache.RemoveAsync($"recepcao:u={usuarioId}:q=all", cancellationToken);
            await cache.RemoveAsync($"recepcao:u={usuarioId}:q=all:v={command.Placa}", cancellationToken);
            await cache.RemoveAsync($"recepcao:u={usuarioId}:q=all:v={veiculoSelecionado.Id}", cancellationToken);
            await cache.RemoveAsync($"estacionamento:u={usuarioId}:q=all:e={estacionamentoId}:z=all", cancellationToken);
            await cache.RemoveAsync($"estacionamento:u={usuarioId}:q=all:e={estacionamentoId}:z={vagaZona}", cancellationToken);
            await cache.RemoveAsync($"estacionamento:u={usuarioId}:q=all:e={estacionamentoNome}:z=all", cancellationToken);
            await cache.RemoveAsync($"estacionamento:u={usuarioId}:q=all:e={estacionamentoNome}:z={vagaZona}", cancellationToken);

            RegistrarSaidaResult result = mapper.Map<RegistrarSaidaResult>(novoRegistroSaida);

            return Result.Ok(result);
        }
        catch (DbUpdateException ex)
        {
            await unitOfWork.RollbackAsync();

            logger.LogError(
                ex,
                "Ocorreu um erro durante o registro de {@Registro}.",
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
    }
}
