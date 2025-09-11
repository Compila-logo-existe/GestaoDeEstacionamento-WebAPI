using FluentResults;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;

public record RegistrarUsuarioCommand(string NomeCompleto, string Email, string Senha, string ConfirmarSenha)
    : IRequest<Result<AccessToken>>;
