using AutoMapper;
using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Handlers;

public class SelecionarRegistrosEntradaQueryHandler(
    IMapper mapper,
    IRepositorioRegistroEntrada repositorioRegistroEntrada,
    ITenantProvider tenantProvider,
    IDistributedCache cache,
    ILogger<SelecionarRegistrosEntradaQueryHandler> logger
) : IRequestHandler<SelecionarRegistrosEntradaQuery, Result<SelecionarRegistrosEntradaResult>>
{
    public async Task<Result<SelecionarRegistrosEntradaResult>> Handle(
        SelecionarRegistrosEntradaQuery query, CancellationToken cancellationToken)
    {
        Guid? usuarioId = tenantProvider.UsuarioId;
        if (usuarioId is null || usuarioId == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário não identificado no tenant."));

        string cacheQuery = query.Quantidade.HasValue ? $"q={query.Quantidade.Value}" : "q=all";
        string cacheKey = $"recepcao:u={usuarioId}:{cacheQuery}";

        string? jsonString = await cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrWhiteSpace(jsonString))
        {
            SelecionarRegistrosEntradaResult? resultadoEmCache = JsonSerializer.Deserialize<SelecionarRegistrosEntradaResult>(jsonString);

            if (resultadoEmCache is not null)
                return Result.Ok(resultadoEmCache);
        }

        try
        {

            List<RegistroEntrada> registros = query.Quantidade.HasValue ?
                await repositorioRegistroEntrada.SelecionarRegistrosAsync(query.Quantidade.Value) :
                await repositorioRegistroEntrada.SelecionarRegistrosAsync();

            SelecionarRegistrosEntradaResult result = mapper.Map<SelecionarRegistrosEntradaResult>(registros);

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
