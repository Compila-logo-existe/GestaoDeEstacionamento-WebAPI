using FluentValidation;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

namespace GestaoDeEstacionamento.Core.Aplicacao.FluentValidation.ModuloEstacionamento;

public class ObterVagaPorIdQueryValidator : AbstractValidator<ObterVagaPorIdQuery>
{
    public ObterVagaPorIdQueryValidator()
    {
        RuleFor(v => v)
            .Must(CompoeExatamenteUmCaminhoEstacionamentoDoRequest)
            .WithMessage("Informe o ID ou o Nome do Estacionamento. Informe apenas um dos caminhos.");

        RuleFor(v => v)
            .Must(CompoeExatamenteUmCaminhoVagaDoRequest)
            .WithMessage("Informe o ID ou os dados da Vaga. Informe apenas um dos caminhos.");

        When(v => v.VagaId.HasValue, () =>
        {
            RuleFor(v => v.VagaNumero)
                .Empty().WithMessage("O número da Vaga não é necessário se houver o Id da Vaga inserida.");

            RuleFor(v => v.VagaZona)
                .Empty().WithMessage("A zona da Vaga não é necessário se houver o Id da Vaga inserida.");
        });
        When(v => !v.VagaId.HasValue, () =>
        {
            RuleFor(v => v.VagaNumero)
            .GreaterThan(0);

            RuleFor(v => v.VagaZona)
            .NotEmpty().MaximumLength(1);
        });
    }

    private bool CompoeExatamenteUmCaminhoEstacionamentoDoRequest(ObterVagaPorIdQuery query)
    {
        bool estacionamentoIdTemValor = query.EstacionamentoId.HasValue && query.EstacionamentoId.Value != Guid.Empty;
        bool nomeEstacionamentoTemValor = !string.IsNullOrWhiteSpace(query.EstacionamentoNome);
        return estacionamentoIdTemValor ^ nomeEstacionamentoTemValor;
    }

    private bool CompoeExatamenteUmCaminhoVagaDoRequest(ObterVagaPorIdQuery query)
    {
        bool vagaIdTemValor = query.VagaId.HasValue && query.VagaId.Value != Guid.Empty;
        bool numeroVagaTemValor = !string.IsNullOrWhiteSpace(query.VagaNumero.ToString());
        bool zonaVagaTemValor = !string.IsNullOrWhiteSpace(query.VagaZona);
        return vagaIdTemValor ^ (numeroVagaTemValor && zonaVagaTemValor);
    }
}