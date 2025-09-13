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
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

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
            Hospede? novoHospede;
            if (command.HospedeId.HasValue && command.HospedeId.Value != Guid.Empty)
            {
                novoHospede = await repositorioHospede.SelecionarRegistroPorIdAsync(command.HospedeId.Value);
                if (novoHospede is null)
                    return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Hóspede não encontrado."));
            }
            else
            {
                string cPFPadronizado = Regex.Replace(command.CPF!, "[^0-9]", "");

                Hospede? hospedeExistente = await repositorioHospede
                    .SelecionarRegistroPorCPFAsync(cPFPadronizado, cancellationToken);

                if (hospedeExistente is not null)
                {
                    novoHospede = hospedeExistente;
                }
                else
                {
                    novoHospede = mapper.Map<Hospede>(command);
                    novoHospede.AderirUsuario(usuarioId.Value);
                    await repositorioHospede.CadastrarRegistroAsync(novoHospede);
                }
            }

            Veiculo? novoVeiculo = await repositorioVeiculo.SelecionarRegistroPorPlacaAsync(command.Placa, cancellationToken);

            if (novoVeiculo is null)
            {
                novoVeiculo = mapper.Map<Veiculo>(command);
                await repositorioVeiculo.CadastrarRegistroAsync(novoVeiculo);
            }

            novoHospede.AderirVeiculo(novoVeiculo);

            RegistroEntrada novoRegistro = mapper.Map<RegistroEntrada>(command);
            novoRegistro.AderirUsuario(usuarioId.Value);
            novoRegistro.AderirHospede(novoHospede);
            novoRegistro.AderirVeiculo(novoVeiculo);
            novoRegistro.GerarNovoTicket();
            novoRegistro.AderirUsuarioAoTicket(usuarioId.Value);

            await repositorioRegistroEntrada.CadastrarRegistroAsync(novoRegistro);

            novoHospede.AderirUsuario(usuarioId.Value);
            novoVeiculo.AderirUsuario(usuarioId.Value);

            await unitOfWork.CommitAsync();

            string cacheKey = $"contatos:u={tenantProvider.UsuarioId.GetValueOrDefault()}:q=all";
            await cache.RemoveAsync(cacheKey, cancellationToken);

            RegistrarEntradaResult result = mapper.Map<RegistrarEntradaResult>(novoRegistro);

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
    }
}
