using FluentValidation;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Commands;

namespace GestaoDeEstacionamento.Core.Aplicacao.FluentValidation.ModuloFaturamento;

public class GerarRelatorioFinanceiroQueryValidator : AbstractValidator<GerarRelatorioFinanceiroQuery>
{
    public GerarRelatorioFinanceiroQueryValidator()
    {
        RuleFor(x => x.DataInicial)
            .NotEmpty()
            .WithMessage("A data inicial é obrigatória.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("A data inicial não pode ser futura.");

        RuleFor(x => x.DataFinal)
            .NotEmpty()
            .WithMessage("A data final é obrigatória.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
            .WithMessage("A data final não pode ser futura.");

        RuleFor(x => x)
            .Must(x => x.DataInicial <= x.DataFinal)
            .WithMessage("A data inicial deve ser menor ou igual à data final.");
    }
}
