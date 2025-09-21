using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using System.Diagnostics.CodeAnalysis;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloHospede;

public class Hospede : EntidadeBase<Hospede>
{
    public string NomeCompleto { get; set; }
    public string CPF { get; set; }
    public string Telefone { get; set; }
    public List<Veiculo> Veiculos { get; set; } = new();
    public List<RegistroEntrada> RegistrosEntrada { get; set; } = new();
    public List<RegistroSaida> RegistrosSaida { get; set; } = new();

    [ExcludeFromCodeCoverage]
    public Hospede() { }
    public Hospede(string nomeCompleto, string cPF, string telefone) : this()
    {
        NomeCompleto = nomeCompleto;
        CPF = cPF;
        Telefone = telefone;
    }

    public void VincularTenant(Guid tenantId) => TenantId = tenantId;

    public bool PossuiVeiculoPorPlaca(string placa)
        => Veiculos.Any(v => v.Placa.Equals(placa, StringComparison.OrdinalIgnoreCase));

    public void AderirVeiculo(Veiculo veiculo)
    {
        if (!PossuiVeiculoPorPlaca(veiculo.Placa))
            Veiculos.Add(veiculo);

        if (veiculo.Hospede is null || veiculo.HospedeId != Id || veiculo.Hospede != this)
            veiculo.AderirHospede(this);
    }

    public override void AtualizarRegistro(Hospede registroEditado)
    {
        NomeCompleto = registroEditado.NomeCompleto;
        CPF = registroEditado.CPF;
        Telefone = registroEditado.Telefone;
    }
}
