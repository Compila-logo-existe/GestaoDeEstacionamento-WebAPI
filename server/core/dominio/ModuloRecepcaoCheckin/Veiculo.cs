using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using System.Diagnostics.CodeAnalysis;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

public class Veiculo : EntidadeBase<Veiculo>
{
    public string Placa { get; set; }
    public string Modelo { get; set; }
    public string Cor { get; set; }
    public Guid HospedeId { get; set; }
    public Hospede Hospede { get; set; } = null!;
    public Vaga Vaga { get; set; } = null!;
    public List<string> Observacoes { get; set; } = new();
    public List<RegistroEntrada> RegistrosEntrada { get; set; } = new();
    public List<RegistroSaida> RegistrosSaida { get; set; } = new();

    [ExcludeFromCodeCoverage]
    public Veiculo() { }
    public Veiculo(string placa, string modelo, string cor,
        Hospede hospede, List<string>? observacoes) : this()
    {
        Placa = placa;
        Modelo = modelo;
        Cor = cor;
        Hospede = hospede;
        if (observacoes != null)
        {
            Observacoes.AddRange(observacoes);
        }
    }

    public void VincularTenant(Guid tenantId) => UsuarioId = tenantId;

    public void AderirHospede(Hospede hospede)
    {
        Hospede = hospede;
        HospedeId = hospede.Id;
        if (hospede.Veiculos.Count > 0 && hospede.Veiculos.Any(v => !v.Id.Equals(Id)))
            hospede.AderirVeiculo(this);
    }

    public override void AtualizarRegistro(Veiculo registroEditado)
    {
        Placa = registroEditado.Placa;
        Modelo = registroEditado.Modelo;
        Cor = registroEditado.Cor;
    }
}
