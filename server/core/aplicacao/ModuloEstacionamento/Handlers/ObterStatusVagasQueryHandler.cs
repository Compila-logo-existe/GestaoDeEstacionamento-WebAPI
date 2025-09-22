using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Handlers;

public class ObterStatusVagasQueryHandler(
    IValidator<ObterStatusVagasQuery> validator,
    IMapper mapper,
    IRepositorioEstacionamento repositorioEstacionamento,
    IRepositorioVaga repositorioVaga,
    ITenantProvider tenantProvider,
    IDistributedCache cache,
    ILogger<ObterStatusVagasQueryHandler> logger
) : IRequestHandler<ObterStatusVagasQuery, Result<ObterStatusVagasResult>>
{
    public async Task<Result<ObterStatusVagasResult>> Handle(
        ObterStatusVagasQuery query, CancellationToken cancellationToken)
    {
        Guid? tenantId = tenantProvider.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Tenant não informado. Envie o header 'X-Tenant-Id'."));

        Guid? usuarioId = tenantProvider.UsuarioId;
        if (!usuarioId.HasValue || usuarioId.Value == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário autenticado não identificado."));

        ValidationResult resultValidation = await validator.ValidateAsync(query, cancellationToken);

        if (!resultValidation.IsValid)
        {
            IEnumerable<string> erros = resultValidation.Errors.Select(e => e.ErrorMessage);
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro(erros));
        }

        query = query with
        {
            Placa = Padronizador.PadronizarPlaca(query.Placa)
        };

        string cacheQueryQuantidade = (query.Quantidade.HasValue && query.Quantidade >= 1) ? $"q={query.Quantidade.Value}" : "q=all";

        string cacheQueryEstacionamento;

        if (query.EstacionamentoId.HasValue)
            cacheQueryEstacionamento = $"e={query.EstacionamentoId.Value}";
        else if (!string.IsNullOrWhiteSpace(query.EstacionamentoNome))
            cacheQueryEstacionamento = $"e={query.EstacionamentoNome}";
        else
            cacheQueryEstacionamento = $"e=all";

        string cacheQueryZona = !string.IsNullOrWhiteSpace(query.Zona) ? $"z={query.Zona}" : "z=all";
        string cacheKey = $"estacionamento:t={tenantId}:{cacheQueryQuantidade}:{cacheQueryEstacionamento}:{cacheQueryZona}";

        string? jsonString = await cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrWhiteSpace(jsonString))
        {
            ObterStatusVagasResult? resultadoEmCache = JsonSerializer.Deserialize<ObterStatusVagasResult>(jsonString);

            if (resultadoEmCache is not null)
                return Result.Ok(resultadoEmCache);
        }

        try
        {

            Estacionamento? estacionamentoSelecionado = null!;

            if (query.EstacionamentoId.HasValue && query.EstacionamentoId.Value != Guid.Empty)
            {
                estacionamentoSelecionado = await repositorioEstacionamento.SelecionarRegistroPorIdAsync(query.EstacionamentoId.Value);
                if (estacionamentoSelecionado is null)
                    return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro($"Estacionamento não encontrado. Id: {query.EstacionamentoId.Value}"));
            }
            else if (!string.IsNullOrWhiteSpace(query.EstacionamentoNome))
            {
                estacionamentoSelecionado = await repositorioEstacionamento.SelecionarRegistroPorNome(query.EstacionamentoNome, tenantId, cancellationToken);
                if (estacionamentoSelecionado is null)
                    return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro($"Estacionamento não encontrado. Nome: {query.EstacionamentoNome}"));
            }

            ZonaEstacionamento? zona = null!;

            if (!string.IsNullOrWhiteSpace(query.Zona) &&
                Enum.TryParse(query.Zona, true, out ZonaEstacionamento zonaConvertida))
            {
                zona = zonaConvertida;
            }

            bool deveAplicarFiltroPorPlaca = !string.IsNullOrWhiteSpace(query.Placa);

            List<Vaga> vagas = (query.Quantidade.HasValue && query.Quantidade >= 1) ?
                await repositorioVaga.SelecionarRegistrosDoEstacionamentoAsync(query.Quantidade.Value, estacionamentoSelecionado.Id, zona, cancellationToken) :
                await repositorioVaga.SelecionarRegistrosDoEstacionamentoAsync(estacionamentoSelecionado.Id, zona, cancellationToken);

            if (deveAplicarFiltroPorPlaca)
            {
                vagas = vagas
                    .Where(vaga => vaga.Veiculo is not null &&
                        (vaga.Veiculo.Placa ?? string.Empty)
                        .ToUpperInvariant()
                        .Contains(query.Placa)
                    ).ToList();
            }

            ObterStatusVagasResult result = mapper.Map<ObterStatusVagasResult>(vagas);

            if (!deveAplicarFiltroPorPlaca)
            {
                string jsonPayload = JsonSerializer.Serialize(result);

                DistributedCacheEntryOptions cacheOptions = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60) };

                await cache.SetStringAsync(cacheKey, jsonPayload, cacheOptions, cancellationToken);
            }

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
