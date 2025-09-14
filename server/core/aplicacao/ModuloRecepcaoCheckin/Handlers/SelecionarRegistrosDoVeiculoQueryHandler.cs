using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Handlers;

public class SelecionarRegistrosDoVeiculoQueryHandler(
    IValidator<SelecionarRegistrosDoVeiculoQuery> validator,
    IMapper mapper,
    IRepositorioRegistroEntrada repositorioRegistroEntrada,
    IRepositorioVeiculo repositorioVeiculo,
    ITenantProvider tenantProvider,
    IDistributedCache cache,
    ILogger<SelecionarRegistrosDoVeiculoQueryHandler> logger
) : IRequestHandler<SelecionarRegistrosDoVeiculoQuery, Result<SelecionarRegistrosDoVeiculoResult>>
{
    public async Task<Result<SelecionarRegistrosDoVeiculoResult>> Handle(
        SelecionarRegistrosDoVeiculoQuery query, CancellationToken cancellationToken)
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

            string cacheQuery = query.Quantidade.HasValue ? $"q={query.Quantidade.Value}" : "q=all";

            string cacheKey = $"contatos:u={tenantProvider.UsuarioId.GetValueOrDefault()}:{cacheQuery}";

            // Tenta buscar dados no cache
            string? jsonString = await cache.GetStringAsync(cacheKey, cancellationToken);

            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                SelecionarRegistrosDoVeiculoResult? resultadoEmCache = JsonSerializer.Deserialize<SelecionarRegistrosDoVeiculoResult>(jsonString);

                if (resultadoEmCache is not null)
                    return Result.Ok(resultadoEmCache);
            }

            // Caso não encontre dados no cache, busca direto no banco de dados
            List<RegistroEntrada> registros = query.Quantidade.HasValue ?
                await repositorioRegistroEntrada.SelecionarRegistrosDoVeiculoAsync(query.Quantidade.Value, veiculoSelecionado.Id, cancellationToken) :
                await repositorioRegistroEntrada.SelecionarRegistrosDoVeiculoAsync(veiculoSelecionado.Id, cancellationToken);

            SelecionarRegistrosDoVeiculoResult result = mapper.Map<SelecionarRegistrosDoVeiculoResult>(registros);

            // Salva os dados atualizados em um novo cache após busca no banco
            string jsonPayload = JsonSerializer.Serialize(result);

            DistributedCacheEntryOptions cacheOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) };

            await cache.SetStringAsync(cacheKey, jsonPayload, cacheOptions, cancellationToken);

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Ocorreu um erro durante a seleção de {@Registros}.",
                query
            );

            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }
}
