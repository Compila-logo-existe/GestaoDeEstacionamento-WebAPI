using FluentValidation;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;
using System.Text.RegularExpressions;

namespace GestaoDeEstacionamento.Core.Aplicacao.FluentValidation.ModuloEstacionamento;

public class ObterStatusVagasQueryValidator : AbstractValidator<ObterStatusVagasQuery>
{
    public ObterStatusVagasQueryValidator()
    {
        RuleFor(v => v)
            .Must(CompoeExatamenteUmCaminhoDoRequest)
            .WithMessage("Informe o ID ou o Nome do estacionamento. Informe apenas um dos caminhos.");

        RuleFor(c => c.Placa)
            .NotEmpty().WithMessage("A Placa do Veículo é obrigatória.")
            .Must(PlacaEhValida).WithMessage("Por favor, insira uma placa válida, ex: ABC1D23 ou ABC1234.");
    }

    private bool CompoeExatamenteUmCaminhoDoRequest(ObterStatusVagasQuery query)
    {
        bool estacionamentoIdTemValor = query.EstacionamentoId.HasValue && query.EstacionamentoId.Value != Guid.Empty;
        bool nomeEstacionamentoTemValor = !string.IsNullOrWhiteSpace(query.EstacionamentoNome);
        return estacionamentoIdTemValor ^ nomeEstacionamentoTemValor;
    }

    private static bool PlacaEhValida(string? input)
    {
        return Regex.IsMatch(input!, "^([A-Z]{3}[0-9]{4}|[A-Z]{3}[0-9][A-Z][0-9]{2})$");
    }
}
