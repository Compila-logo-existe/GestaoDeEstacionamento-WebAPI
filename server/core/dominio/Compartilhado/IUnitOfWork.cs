namespace GestaoDeEstacionamento.Core.Dominio.Compartilhado;

public interface IUnitOfWork
{
    public Task CommitAsync();
    public Task RollbackAsync();
}
