using FluentValidation;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;
using System.Text.RegularExpressions;

namespace GestaoDeEstacionamento.Core.Aplicacao.FluentValidation.ModuloRecepcaoCheckin;

public class RegistrarEntradaCommandValidator : AbstractValidator<RegistrarEntradaCommand>
{
    public RegistrarEntradaCommandValidator()
    {
        RuleFor(c => c.NomeCompleto)
            .NotEmpty().WithMessage("O nome do Hóspede é obrigatório.")
            .MinimumLength(2).WithMessage("O nome deve ter pelo menos {MinLength} caracteres.")
            .MaximumLength(150).WithMessage("O nome deve ter pelo menos {MaxLength} caracteres.")
            .Must(NomeEhValido).WithMessage("Por favor, insira um nome válido.");

        RuleFor(c => c.CPF)
            .NotEmpty().WithMessage("O C.P.F. do Hóspede é obrigatório.")
            .Must(CPFEhValido).WithMessage("Por favor, insira um C.P.F. válido.");

        RuleFor(c => c.Telefone)
            .NotEmpty().WithMessage("O telefone do Hóspede é obrigatório..")
            .Must(TelefoneEhValido).WithMessage("Por favor, insira um número de telefone válido, ex: (99) 99999-9999 ou (99) 9999-9999.");

        RuleFor(c => c.Placa)
            .NotEmpty().WithMessage("A Placa do Veículo é obrigatória.");

        RuleFor(c => c.Modelo)
            .NotEmpty().WithMessage("O Modelo do Veículo é obrigatório.");

        RuleFor(c => c.Cor)
            .NotEmpty().WithMessage("A Cor do Veículo é obrigatória.");
    }

    private static bool NomeEhValido(string input)
    {
        return Regex.IsMatch(input, "^[a-zA-ZÄäÖöÜüÀàÈèÌìÒòÙùÁáÉéÍíÓóÚúÝýÂâÊêÎîÔôÛûÃãÑñÇç'\\-\\s]+$");
    }

    private static bool CPFEhValido(string input)
    {
        return Regex.IsMatch(input, "^[0-9]{3}[\\.]?[0-9]{3}[\\.]?[0-9]{3}[-]?[0-9]{2}$");
    }

    private static bool TelefoneEhValido(string input)
    {
        return Regex.IsMatch(input, "^\\(?\\d{2}\\)?\\s?(9\\d{4}|\\d{4})-?\\d{4}$");
    }
}
