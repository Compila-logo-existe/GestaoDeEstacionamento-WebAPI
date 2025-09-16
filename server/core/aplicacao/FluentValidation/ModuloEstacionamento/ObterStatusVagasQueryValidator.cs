using FluentValidation;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

namespace GestaoDeEstacionamento.Core.Aplicacao.FluentValidation.ModuloEstacionamento;

public class ObterStatusVagasQueryValidator : AbstractValidator<ObterStatusVagasQuery>
{
    public ObterStatusVagasQueryValidator()
    {
        RuleFor(v => v)
            .Must(CompoeExatamenteUmCaminhoDoRequest)
            .WithMessage("Informe o ID ou o Nome do estacionamento. Informe apenas um dos caminhos.");
    }

    private bool CompoeExatamenteUmCaminhoDoRequest(ObterStatusVagasQuery query)
    {
        bool estacionamentoIdTemValor = query.EstacionamentoId.HasValue && query.EstacionamentoId.Value != Guid.Empty;
        bool nomeEstacionamentoTemValor = !string.IsNullOrWhiteSpace(query.EstacionamentoNome);
        return estacionamentoIdTemValor ^ nomeEstacionamentoTemValor;
    }
}
