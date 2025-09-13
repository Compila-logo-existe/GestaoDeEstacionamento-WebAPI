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
        ValidationResult resultValidation = await validator.ValidateAsync(command, cancellationToken);

        if (!resultValidation.IsValid)
        {
            IEnumerable<string> erros = resultValidation.Errors.Select(e => e.ErrorMessage);
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro(erros));
        }

        Guid? usuarioId = tenantProvider.UsuarioId;
        if (usuarioId is null || usuarioId == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário não identificado no tenant."));

        try
        {
            Hospede novoHospede = mapper.Map<Hospede>(command);
            novoHospede.DefinirUsuario(usuarioId.Value); // Criar cadastro de hospede

            Veiculo novoVeiculo = mapper.Map<Veiculo>(command);
            novoVeiculo.DefinirUsuario(usuarioId.Value);

            novoHospede.AderirVeiculo(novoVeiculo);
            await repositorioHospede.CadastrarRegistroAsync(novoHospede);

            novoVeiculo.AderirHospede(novoHospede);
            await repositorioVeiculo.CadastrarRegistroAsync(novoVeiculo);

            RegistroEntrada novoRegistro = mapper.Map<RegistroEntrada>(command);
            novoRegistro.DefinirUsuario(usuarioId.Value);
            novoRegistro.DefinirHospede(novoHospede);
            novoRegistro.DefinirVeiculo(novoVeiculo);
            novoRegistro.GerarNovoTicket();
            novoRegistro.DefinirUsuarioTicket(usuarioId.Value);

            await repositorioRegistroEntrada.CadastrarRegistroAsync(novoRegistro);

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
