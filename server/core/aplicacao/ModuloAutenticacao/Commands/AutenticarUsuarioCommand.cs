using FluentResults;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;

public record AutenticarUsuarioCommand(string Email, string Senha) : IRequest<Result<AccessToken>>;
