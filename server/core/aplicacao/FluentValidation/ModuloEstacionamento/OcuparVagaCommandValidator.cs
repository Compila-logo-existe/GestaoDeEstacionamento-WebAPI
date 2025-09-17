using FluentValidation;
using GestaoDeEstacionamento.Core.Aplicacao.ModuloEstacionamento.Commands;

namespace GestaoDeEstacionamento.Core.Aplicacao.FluentValidation.ModuloEstacionamento;

public class OcuparVagaCommandValidator : AbstractValidator<OcuparVagaCommand>
{
    public OcuparVagaCommandValidator()
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
            .NotEmpty().MaximumLength(200);
        });

    }

    private bool CompoeExatamenteUmCaminhoEstacionamentoDoRequest(OcuparVagaCommand command)
    {
        bool estacionamentoIdTemValor = command.EstacionamentoId.HasValue && command.EstacionamentoId.Value != Guid.Empty;
        bool nomeEstacionamentoTemValor = !string.IsNullOrWhiteSpace(command.EstacionamentoNome);
        return estacionamentoIdTemValor ^ nomeEstacionamentoTemValor;
    }

    private bool CompoeExatamenteUmCaminhoVagaDoRequest(OcuparVagaCommand command)
    {
        bool vagaIdTemValor = command.VagaId.HasValue && command.VagaId.Value != Guid.Empty;
        bool numeroVagaTemValor = !string.IsNullOrWhiteSpace(command.VagaNumero.ToString());
        bool zonaVagaTemValor = !string.IsNullOrWhiteSpace(command.VagaZona);
        return vagaIdTemValor ^ (numeroVagaTemValor && zonaVagaTemValor);
    }
}
