using FluentResults;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;
using GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Handlers;

public class SairCommandHandler(
    SignInManager<Usuario> signInManager
) : IRequestHandler<SairCommand, Result>
{
    public async Task<Result> Handle(SairCommand request, CancellationToken cancellationToken)
    {
        await signInManager.SignOutAsync();

        return Result.Ok();
    }
}
