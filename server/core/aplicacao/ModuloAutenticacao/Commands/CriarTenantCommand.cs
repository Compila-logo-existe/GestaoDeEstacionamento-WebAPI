using FluentResults;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;

public record CriarTenantCommand(
    string Nome,
    string? CNPJ,
    string SlugSubdominio,
    string? DominioPersonalizado
) : IRequest<Result<Guid>>;
