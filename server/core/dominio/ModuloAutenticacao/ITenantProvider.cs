namespace GestaoDeEstacionamento.Core.Dominio.ModuloAutenticacao;

public interface ITenantProvider
{
    Guid? UsuarioId { get; }
    Guid? TenantId { get; }
    string? Slug { get; }
    bool IsInRole(string cargo);
}
