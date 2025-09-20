using GestaoDeEstacionamento.Core.Dominio.Compartilhado;
using GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloEstacionamento;

public class Vaga : EntidadeBase<Vaga>
{
    public int Numero { get; set; }
    public ZonaEstacionamento Zona { get; set; }
    public Guid? VeiculoId { get; set; }
    public Veiculo? Veiculo { get; set; }
    public StatusVaga Status => Veiculo is null ? StatusVaga.Livre : StatusVaga.Ocupada;
    public Guid EstacionamentoId { get; set; }
    public Estacionamento Estacionamento { get; set; } = null!;
    public bool EstaOcupada => Veiculo is not null;

    public void VincularTenant(Guid tenantId) => UsuarioId = tenantId;

    public void Ocupar(Veiculo veiculo)
    {
        if (Veiculo is not null)
            return;

        Veiculo = veiculo;
        VeiculoId = veiculo.Id;
    }

    public void Liberar()
    {
        if (Veiculo is null)
            return;

        Veiculo = null;
        VeiculoId = null;
    }

    public override void AtualizarRegistro(Vaga registroEditado)
    {
        Numero = registroEditado.Numero;
        Zona = registroEditado.Zona;
    }
}
