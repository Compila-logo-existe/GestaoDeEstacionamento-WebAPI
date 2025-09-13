using FluentValidation;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;

namespace GestaoDeEstacionamento.Core.Aplicacao.FluentValidation.ModuloRecepcaoCheckin;

public class RegistrarEntradaCommandValidator : AbstractValidator<RegistrarEntradaCommand>
{
    public RegistrarEntradaCommandValidator()
    {
        RuleFor(c => c.Placa)
            .NotEmpty().WithMessage("A Placa é obrigatória.");
        RuleFor(c => c.Modelo)
            .NotEmpty().WithMessage("O Modelo é obrigatório.");
        RuleFor(c => c.Cor)
            .NotEmpty().WithMessage("A Cor é obrigatória.");
    }
}
