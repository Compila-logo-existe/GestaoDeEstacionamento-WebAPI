using FluentResults;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;

public record RotacionarTokenCommand(string RefreshTokenString)
    : IRequest<Result<(AccessToken AccessToken, string RefreshToken)>>;
