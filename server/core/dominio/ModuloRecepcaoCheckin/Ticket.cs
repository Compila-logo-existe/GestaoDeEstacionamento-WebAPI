namespace GestaoDeEstacionamento.Core.Dominio.ModuloRecepcaoCheckin;

public class Ticket
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public DateTime EmissaoEmUtc { get; init; } = DateTime.UtcNow;
    public int NumeroSequencial { get; set; }
    public RegistroEntrada RegistroEntrada { get; set; } = null!;

    public Ticket() { }
    public Ticket(RegistroEntrada registroEntrada) : this()
    {
        RegistroEntrada = registroEntrada;
    }
}
