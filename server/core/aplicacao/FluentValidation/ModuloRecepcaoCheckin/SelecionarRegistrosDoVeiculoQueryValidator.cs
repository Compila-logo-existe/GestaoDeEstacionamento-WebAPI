using FluentValidation;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloRecepcaoCheckin.Commands;

namespace GestaoDeEstacionamento.Core.Aplicacao.FluentValidation.ModuloRecepcaoCheckin;

public class SelecionarRegistrosDoVeiculoQueryValidator : AbstractValidator<SelecionarRegistrosDoVeiculoQuery>
{
    public SelecionarRegistrosDoVeiculoQueryValidator()
    {
        RuleFor(c => c)
            .Must(CompoeExatamenteUmCaminhoDoRequest)
            .WithMessage("Informe o ID ou a Placa do veículo. Informe apenas um dos caminhos.");
    }

    private static bool CompoeExatamenteUmCaminhoDoRequest(SelecionarRegistrosDoVeiculoQuery query)
    {
        bool veiculoIdTemValor = query.VeiculoId.HasValue && query.VeiculoId.Value != Guid.Empty;
        bool placaVeiculoTemValor = !string.IsNullOrWhiteSpace(query.Placa);
        return veiculoIdTemValor ^ placaVeiculoTemValor; // apenas um dos dois booleanos precisam ser validos.
    }
}
