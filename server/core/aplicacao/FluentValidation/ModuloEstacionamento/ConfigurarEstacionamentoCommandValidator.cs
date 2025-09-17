using FluentValidation;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using System.Text.RegularExpressions;

namespace GestaoDeEstacionamento.Core.Aplicacao.FluentValidation.ModuloEstacionamento;

public class ConfigurarEstacionamentoCommandValidator : AbstractValidator<ConfigurarEstacionamentoCommand>
{
    public ConfigurarEstacionamentoCommandValidator()
    {
        RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome do estacionamento é obrigatório.")
                .MinimumLength(2).WithMessage("O nome deve ter pelo menos {MinLength} caracteres.")
                .MaximumLength(200).WithMessage("O nome deve ter pelo menos máximo {MaxLength} caracteres.")
                .Must(NomeEhValido).WithMessage("Por favor, insira um nome válido.");

        RuleFor(c => c.QuantidadeVagas)
            .InclusiveBetween(1, 100)
            .WithMessage("A quantidade de vagas precisa ser de 1 a 100.");

        RuleFor(x => x.ZonasTotais)
            .GreaterThan(0)
            .LessThanOrEqualTo(26)
            .WithMessage("ZonasTotais deve estar entre 1 e 26 (A-Z).");

        RuleFor(x => x.VagasPorZona)
            .GreaterThan(0);

        RuleFor(x => x)
            .Must(x => (long)x.ZonasTotais * x.VagasPorZona >= x.QuantidadeVagas)
            .WithMessage("Capacidade insuficiente: ZonasTotais * VagasPorZona deve ser >= QuantidadeVagas.");

    }

    private static bool NomeEhValido(string input)
    {
        return Regex.IsMatch(input, "^[a-zA-ZÄäÖöÜüÀàÈèÌìÒòÙùÁáÉéÍíÓóÚúÝýÂâÊêÎîÔôÛûÃãÑñÇç'\\-\\s0-9]+$");
    }
}
