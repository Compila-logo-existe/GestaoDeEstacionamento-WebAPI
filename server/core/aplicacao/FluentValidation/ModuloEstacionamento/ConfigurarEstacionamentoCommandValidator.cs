using FluentValidation;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

namespace GestaoDeEstacionamento.Core.Aplicacao.FluentValidation.ModuloEstacionamento;

public class ConfigurarEstacionamentoCommandValidator : AbstractValidator<ConfigurarEstacionamentoCommand>
{
    public ConfigurarEstacionamentoCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().MaximumLength(200);

        RuleFor(c => c.QuantidadeVagas)
            .InclusiveBetween(1, 100)
            .WithMessage("A quantidade de vagas precisa ser de 1 a 100.");

        RuleFor(x => x.ZonasTotais)
            .GreaterThan(0)
            .LessThanOrEqualTo(26)
            .WithMessage("ZonasTotais deve estar entre 1 e 26.");

        RuleFor(x => x.VagasPorZona)
            .GreaterThan(0);

        RuleFor(x => x)
            .Must(x => (long)x.ZonasTotais * x.VagasPorZona >= x.QuantidadeVagas)
            .WithMessage("Capacidade insuficiente: ZonasTotais * VagasPorZona deve ser >= QuantidadeVagas.");

    }
}
