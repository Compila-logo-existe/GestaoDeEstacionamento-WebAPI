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
using System.Security.Cryptography;
using System.Text;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Handlers;

public class RegistrarEntradaCommandHandler(
    IValidator<RegistrarEntradaCommand> validator,
    IMapper mapper,
    IRepositorioHospede repositorioHospede,
    IRepositorioVeiculo repositorioVeiculo,
    IRepositorioRegistroEntrada repositorioRegistroEntrada,
    ITenantProvider tenantProvider,
    IDistributedCache cache,
    IUnitOfWork unitOfWork,
    ILogger<RegistrarEntradaCommandHandler> logger
) : IRequestHandler<RegistrarEntradaCommand, Result<RegistrarEntradaResult>>
{
    public async Task<Result<RegistrarEntradaResult>> Handle(
        RegistrarEntradaCommand command, CancellationToken cancellationToken)
    {
        Guid? tenantId = tenantProvider.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Tenant não informado. Envie o header 'X-Tenant-Id'."));

        Guid? usuarioId = tenantProvider.UsuarioId;
        if (!usuarioId.HasValue || usuarioId.Value == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário autenticado não identificado."));

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
            Hospede? novoHospede;
            if (command.HospedeId.HasValue && command.HospedeId.Value != Guid.Empty)
            {
                novoHospede = await repositorioHospede.SelecionarRegistroPorIdAsync(command.HospedeId.Value);
                if (novoHospede is null)
                    return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro("Hóspede não encontrado."));
            }
            else
            {
                Hospede? hospedeExistente = await repositorioHospede.SelecionarRegistroPorCPFAsync(command.CPF, tenantId, cancellationToken);

                if (hospedeExistente is not null)
                {
                    novoHospede = hospedeExistente;
                }
                else
                {
                    novoHospede = mapper.Map<Hospede>(command);
                    await repositorioHospede.CadastrarRegistroAsync(novoHospede);
                }
            }

            Veiculo? novoVeiculo = await repositorioVeiculo.SelecionarRegistroPorPlacaAsync(command.Placa, tenantId, cancellationToken);

            if (novoVeiculo is null)
            {
                novoVeiculo = mapper.Map<Veiculo>(command);
                novoVeiculo.VincularTenant(tenantId.Value);
                await repositorioVeiculo.CadastrarRegistroAsync(novoVeiculo);
            }

            novoHospede.AderirVeiculo(novoVeiculo);

            bool possuiEntradaEmAberto = await repositorioRegistroEntrada.ExisteAberturaPorPlacaAsync(command.Placa, tenantId, cancellationToken);
            if (possuiEntradaEmAberto)
                return Result.Fail(ResultadosErro.ConflitoErro("Já existe check-in/ticket em aberto para esta placa."));

            RegistroEntrada novoRegistro = mapper.Map<RegistroEntrada>(command);
            novoRegistro.VincularTenant(tenantId.Value);
            novoRegistro.AderirHospede(novoHospede);
            novoRegistro.AderirVeiculo(novoVeiculo);
            novoRegistro.GerarNovoTicket();
            novoRegistro.GerarNovoFaturamento(command.ValorDiaria);
            novoRegistro.VincularTenantAoTicket(tenantId.Value);

            await repositorioRegistroEntrada.CadastrarRegistroAsync(novoRegistro);

            novoHospede.VincularTenant(tenantId.Value);
            novoVeiculo.VincularTenant(tenantId.Value);

            await unitOfWork.CommitAsync();

            string placaPadronizadaParaCache = Padronizador.PadronizarPlaca(command.Placa);
            string placaHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(placaPadronizadaParaCache))
            );

            await cache.RemoveAsync($"recepcao:t={tenantId}:q=all", cancellationToken);
            await cache.RemoveAsync($"recepcao:t={tenantId}:q=all:v={placaHash}", cancellationToken);
            await cache.RemoveAsync($"recepcao:t={tenantId}:q=all:v={novoVeiculo.Id}", cancellationToken);

            RegistrarEntradaResult result = mapper.Map<RegistrarEntradaResult>(novoRegistro);

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
