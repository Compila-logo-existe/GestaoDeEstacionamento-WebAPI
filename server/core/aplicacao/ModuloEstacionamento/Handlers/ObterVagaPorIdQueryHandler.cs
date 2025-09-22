using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using GestaoDeEstacionamento.Core.Aplicacao.Compartilhado;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Handlers;

public class ObterVagaPorIdQueryHandler(
    IValidator<ObterVagaPorIdQuery> validator,
    IMapper mapper,
    IRepositorioEstacionamento repositorioEstacionamento,
    IRepositorioVaga repositorioVaga,
    ITenantProvider tenantProvider,
    ILogger<ObterVagaPorIdQueryHandler> logger
) : IRequestHandler<ObterVagaPorIdQuery, Result<ObterVagaPorIdResult>>
{
    public async Task<Result<ObterVagaPorIdResult>> Handle(
        ObterVagaPorIdQuery query, CancellationToken cancellationToken)
    {
        Guid? tenantId = tenantProvider.TenantId;
        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Tenant não informado. Envie o header 'X-Tenant-Id'."));

        Guid? usuarioId = tenantProvider.UsuarioId;
        if (!usuarioId.HasValue || usuarioId.Value == Guid.Empty)
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro("Usuário autenticado não identificado."));

        ValidationResult valid = await validator.ValidateAsync(query, cancellationToken);

        if (!valid.IsValid)
        {
            IEnumerable<string> erros = valid.Errors.Select(e => e.ErrorMessage);
            return Result.Fail(ResultadosErro.RequisicaoInvalidaErro(erros));
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

            Vaga? vagaSelecionada = null!;
            if (query.VagaId.HasValue)
            {
                vagaSelecionada = await repositorioVaga.SelecionarRegistroPorIdAsync(query.VagaId.Value);
                if (vagaSelecionada is null)
                    return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro($"Vaga não encontrada. Id: {query.VagaId.Value}"));
            }
            else if (query.VagaNumero.HasValue && !string.IsNullOrWhiteSpace(query.VagaZona))
            {

                ZonaEstacionamento? zona = null!;

                if (!string.IsNullOrWhiteSpace(query.VagaZona) &&
                    Enum.TryParse(query.VagaZona, true, out ZonaEstacionamento zonaConvertida))
                {
                    zona = zonaConvertida;
                }

                vagaSelecionada = await repositorioVaga.SelecionarRegistroPorDadosAsync(query.VagaNumero.Value, zona, estacionamentoSelecionado.Id, tenantId, cancellationToken);
                if (vagaSelecionada is null)
                    return Result.Fail(ResultadosErro.RegistroNaoEncontradoErro($"Vaga não encontrada. Dados: {query.VagaZona}-{query.VagaNumero}"));
            }

            if (!vagaSelecionada.Estacionamento.Equals(estacionamentoSelecionado))
                return Result.Fail(ResultadosErro.ConflitoErro("A vaga escolhida não pertence a este estacionaento."));

            ObterVagaPorIdResult result = mapper.Map<ObterVagaPorIdResult>(vagaSelecionada);

            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao obter vaga por Id. {@Query}", query);
            return Result.Fail(ResultadosErro.ExcecaoInternaErro(ex));
        }
    }
}
