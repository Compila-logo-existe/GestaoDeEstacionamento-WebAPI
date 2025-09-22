using FluentValidation;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using System.Text.RegularExpressions;

namespace GestaoDeEstacionamento.Core.Aplicacao.FluentValidation.ModuloRecepcaoCheckin;

public class RegistrarSaidaCommandValidator : AbstractValidator<RegistrarSaidaCommand>
{
    public RegistrarSaidaCommandValidator()
    {
        RuleFor(v => v)
            .Must(CompoeExatamenteUmCaminhoHospedeDoRequest)
            .WithMessage("Informe o ID do Hóspede OU o C.P.F. Informe apenas um dos caminhos.");

        RuleFor(v => v)
            .Must(CompoeExatamenteUmCaminhoVeiculoDoRequest)
            .WithMessage("Informe o ID ou a Placa do Veículo. Informe apenas um dos caminhos.");

        When(c => c.HospedeId.HasValue && c.HospedeId.Value != Guid.Empty, () =>
        {
            RuleFor(c => c.CPF)
                .Empty().WithMessage("O C.P.F. do Hóspede não é necessário se houver o Id do Hóspede inserido.");
        });
        When(c => !c.HospedeId.HasValue || c.HospedeId.Value == Guid.Empty, () =>
        {
            RuleFor(c => c.CPF)
                .NotEmpty().WithMessage("O C.P.F. do Hóspede é obrigatório.")
                .Must(CPFEhValido).WithMessage("Por favor, insira um C.P.F. válido.");
        });

        When(c => c.VeiculoId.HasValue && c.VeiculoId.Value != Guid.Empty, () =>
        {
            RuleFor(c => c.Placa)
                .Empty().WithMessage("A Placa do Veículo não é necessária se houver o Id do Veículo inserido.");
        });
        When(c => !c.HospedeId.HasValue || c.HospedeId.Value == Guid.Empty, () =>
        {
            RuleFor(c => c.Placa)
                .NotEmpty().WithMessage("A Placa do Veículo é obrigatória.")
                .Must(PlacaEhValida).WithMessage("Por favor, insira uma placa válida, ex: ABC1D23 ou ABC1234.");
        });

    }
    private static bool CompoeExatamenteUmCaminhoHospedeDoRequest(RegistrarSaidaCommand command)
    {
        bool hospedeIdTemValor = command.HospedeId.HasValue && command.HospedeId.Value != Guid.Empty;
        bool cPFHospedeTemValor = !string.IsNullOrWhiteSpace(command.CPF);
        return hospedeIdTemValor ^ cPFHospedeTemValor;
    }

    private static bool CompoeExatamenteUmCaminhoVeiculoDoRequest(RegistrarSaidaCommand command)
    {
        bool veiculoIdTemValor = command.VeiculoId.HasValue && command.VeiculoId.Value != Guid.Empty;
        bool placaVeiculoTemValor = !string.IsNullOrWhiteSpace(command.Placa);
        return veiculoIdTemValor ^ placaVeiculoTemValor;
    }

    private static bool CPFEhValido(string? input)
    {
        return Regex.IsMatch(input!, "^[0-9]{3}[\\.]?[0-9]{3}[\\.]?[0-9]{3}[-]?[0-9]{2}$");
    }

    private static bool PlacaEhValida(string? input)
    {
        return Regex.IsMatch(input!, "^([A-Z]{3}[0-9]{4}|[A-Z]{3}[0-9][A-Z][0-9]{2})$");
    }
}
