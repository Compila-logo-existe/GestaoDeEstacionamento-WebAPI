using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;
using System.Diagnostics.CodeAnalysis;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloHospede;

public class Hospede : EntidadeBase<Hospede>
{
    public string NomeCompleto { get; set; }
    public string CPF { get; set; }
    public string Telefone { get; set; }
    public Veiculo Veiculo { get; set; }
    public List<RegistroEntrada> RegistrosEntrada { get; set; } = new();

    [ExcludeFromCodeCoverage]
    public Hospede() { }
    public Hospede(string nomeCompleto, string cPF, string telefone) : this()
    {
        NomeCompleto = nomeCompleto;
        CPF = cPF;
        Telefone = telefone;
    }

    public void DefinirUsuario(Guid usuarioId) => UsuarioId = usuarioId;

    public void AderirVeiculo(Veiculo veiculo)
    {
        Veiculo = veiculo;
        if (veiculo.Hospede is not null && (veiculo.HospedeId != Id || veiculo.Hospede != this))
            veiculo.AderirHospede(this);
    }

    public override void AtualizarRegistro(Hospede registroEditado)
    {
        NomeCompleto = registroEditado.NomeCompleto;
        CPF = registroEditado.CPF;
        Telefone = registroEditado.Telefone;
    }
}
