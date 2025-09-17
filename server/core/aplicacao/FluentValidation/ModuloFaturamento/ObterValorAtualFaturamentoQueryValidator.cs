using FluentValidation;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloFaturamento.Commands;

namespace GestaoDeEstacionamento.Core.Aplicacao.FluentValidation.ModuloFaturamento;

public class ObterValorAtualFaturamentoQueryValidator : AbstractValidator<ObterValorAtualFaturamentoQuery>
{
    public ObterValorAtualFaturamentoQueryValidator()
    {
        RuleFor(f => f)
            .Must(CompoeExatamenteUmCaminhoEstacionamentoDoRequest)
            .WithMessage("Informe o ID ou o Nome do Estacionamento. Informe apenas um dos caminhos.");

        RuleFor(f => f)
            .Must(CompoeExatamenteUmCaminhoDoRequest)
            .WithMessage("Informe o Número do Ticket ou a Placa do Veículo. Informe apenas um dos caminhos.");
    }

    private bool CompoeExatamenteUmCaminhoEstacionamentoDoRequest(ObterValorAtualFaturamentoQuery query)
    {
        bool estacionamentoIdTemValor = query.EstacionamentoId.HasValue && query.EstacionamentoId.Value != Guid.Empty;
        bool nomeEstacionamentoTemValor = !string.IsNullOrWhiteSpace(query.EstacionamentoNome);
        return estacionamentoIdTemValor ^ nomeEstacionamentoTemValor;
    }

    private bool CompoeExatamenteUmCaminhoDoRequest(ObterValorAtualFaturamentoQuery query)
    {
        bool numeroTicketTemValor = !string.IsNullOrWhiteSpace(query.NumeroSequencialDoTicket.ToString());
        bool placaVeiculoTemValor = !string.IsNullOrWhiteSpace(query.Placa);
        return numeroTicketTemValor ^ placaVeiculoTemValor;
    }
}
