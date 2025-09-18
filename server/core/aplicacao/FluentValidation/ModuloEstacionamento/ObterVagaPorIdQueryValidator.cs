using FluentValidation;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

namespace GestaoDeEstacionamento.Core.Aplicacao.FluentValidation.ModuloEstacionamento;

public class ObterVagaPorIdQueryValidator : AbstractValidator<ObterVagaPorIdQuery>
{
    public ObterVagaPorIdQueryValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .Must(x => x.VagaId != Guid.Empty)
            .WithMessage("Informe o Id da Vaga válido.");
    }
}