using FluentResults;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;

public record AceitarConviteCommand(
    string TokenConvite,
    string NomeCompleto,
    string Senha,
    string ConfirmarSenha
) : IRequest<Result<AccessToken>>;
