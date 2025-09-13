using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloHospede;
using System.Diagnostics.CodeAnalysis;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

public class Veiculo : EntidadeBase<Veiculo>
{
    public string Placa { get; set; }
    public string Modelo { get; set; }
    public string Cor { get; set; }
    public Guid HospedeId { get; set; }
    public Hospede Hospede { get; set; }
    public List<RegistroEntrada> RegistrosEntrada { get; set; } = new();

    [ExcludeFromCodeCoverage]
    public Veiculo() { }
    public Veiculo(string placa, string modelo, string cor,
        Hospede? hospede) : this()
    {
        Placa = placa;
        Modelo = modelo;
        Cor = cor;
        Hospede = hospede;
    }

    public void AderirUsuario(Guid usuarioId) => UsuarioId = usuarioId;

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
