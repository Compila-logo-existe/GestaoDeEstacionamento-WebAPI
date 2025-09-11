using FluentResults;
using MediatR;

namespace GestaoDeEstacionamento.Core.Aplicacao.ModuloAutenticacao.Commands;

public record SairCommand : IRequest<Result>;
