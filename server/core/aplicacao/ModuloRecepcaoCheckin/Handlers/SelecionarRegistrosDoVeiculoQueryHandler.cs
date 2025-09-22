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
using System.Security.Cryptography;
using System.Text;
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
        Guid? tenantId = tenantProvider.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Tenant não informado. Envie o header 'X-Tenant-Id'."));

        Guid? usuarioId = tenantProvider.UsuarioId;
        if (!usuarioId.HasValue || usuarioId.Value == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário autenticado não identificado."));

        string cacheQueryQuantidade = (query.Quantidade.HasValue && query.Quantidade >= 1) ? $"q={query.Quantidade.Value}" : "q=all";

        string cacheQueryVeiculo;
        if (query.VeiculoId.HasValue && query.VeiculoId.Value != Guid.Empty)
        {
            cacheQueryVeiculo = $"v={query.VeiculoId.Value}";
        }
        else if (!string.IsNullOrWhiteSpace(query.Placa))
        {
            string placaPadronizadaParaCache = Padronizador.PadronizarPlaca(query.Placa);
            string placaHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(placaPadronizadaParaCache))
            );

            cacheQueryVeiculo = $"v={placaHash}";
        }
        else
        {
            cacheQueryVeiculo = "v=all";
        }

        string cacheKey = $"recepcao:t={tenantId}:{cacheQueryQuantidade}:{cacheQueryVeiculo}";

        string? jsonString = await cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrWhiteSpace(jsonString))
        {
            SelecionarRegistrosDoVeiculoResult? resultadoEmCache =
                JsonSerializer.Deserialize<SelecionarRegistrosDoVeiculoResult>(jsonString);

            if (resultadoEmCache is not null)
                return Result.Ok(resultadoEmCache);
        }

        ValidationResult resultValidation = await validator.ValidateAsync(query, cancellationToken);

        if (!resultValidation.IsValid)
        {
            IEnumerable<string> erros = resultValidation.Errors.Select(e => e.ErrorMessage);
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro(erros));
        }

        try
        {

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

                veiculoSelecionado = await repositorioVeiculo.SelecionarRegistroPorPlacaAsync(query.Placa, tenantId, cancellationToken);
                if (veiculoSelecionado is null)
                    return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro($"Veículo não encontrado. Placa: {query.Placa}"));
            }

            List<RegistroEntrada> registros = (query.Quantidade.HasValue && query.Quantidade >= 1) ?
                await repositorioRegistroEntrada.SelecionarRegistrosDoVeiculoAsync(query.Quantidade.Value, veiculoSelecionado.Id, cancellationToken) :
                await repositorioRegistroEntrada.SelecionarRegistrosDoVeiculoAsync(veiculoSelecionado.Id, cancellationToken);

            SelecionarRegistrosDoVeiculoResult result = mapper.Map<SelecionarRegistrosDoVeiculoResult>(registros);

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
