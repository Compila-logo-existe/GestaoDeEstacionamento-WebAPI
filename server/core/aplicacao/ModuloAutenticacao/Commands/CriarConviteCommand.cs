using FluentResults;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;

public record CriarConviteCommand(
    Guid TenantId,
    string EmailConvidado,
    string NomeCargo
) : IRequest<Result<(string TokenConvite, DateTime ExpiraEmUtc)>>;
