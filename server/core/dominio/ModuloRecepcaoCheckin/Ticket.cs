using GestaoDeEstacionamento.Core.Dominio.Compartilhado;

namespace GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

public class Ticket : EntidadeBase<Ticket>
{
    public DateTime EmissaoEmUtc { get; init; } = DateTime.UtcNow;
    public int NumeroSequencial { get; set; }
    public StatusTicket Status => RegistroSaida is null ? StatusTicket.Valido : StatusTicket.Expirado;
    public RegistroEntrada RegistroEntrada { get; set; } = null!;
    public RegistroSaida RegistroSaida { get; set; } = null!;

    public Ticket() { }
    public Ticket(RegistroEntrada registroEntrada) : this()
    {
        RegistroEntrada = registroEntrada;
    }

    public void AderirRegistroSaida(RegistroSaida registroSaida) => RegistroSaida = registroSaida;

    public DateTime ObterDataEntrada() => RegistroEntrada.DataEntradaEmUtc;

    public DateTime? ObterDataSaida() => RegistroSaida.DataSaidaEmUtc;

    public override void AtualizarRegistro(Ticket registroEditado)
    {
        throw new NotImplementedException();
    }
}
