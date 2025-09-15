using FluentValidation;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

namespace GestaoDeEstacionamento.Core.Aplicacao.FluentValidation.ModuloEstacionamento;

public class ConfigurarEstacionamentoCommandValidator : AbstractValidator<ConfigurarEstacionamentoCommand>
{
    public ConfigurarEstacionamentoCommandValidator()
    {
        RuleFor(c => c.QuantidadeVagas)
            .InclusiveBetween(1, 100)
            .WithMessage("A quantidade de vagas precisa ser de 1 a 100.");
    }
}
