namespace GestaoDeEstacionamento.Core.Dominio.Compartilhado;

public interface IRepositorio<T> where T : EntidadeBase<T>
{
    public Task CadastrarRegistroAsync(T novoRegistro);

    public Task<bool> EditarRegistroAsync(Guid idRegistro, T registroEditado);

    public Task<bool> ExcluirRegistroAsync(Guid idRegistro);

    public Task<List<T>> SelecionarRegistrosAsync();

    public Task<List<T>> SelecionarRegistrosAsync(int quantidade);

    public Task<T?> SelecionarRegistroPorIdAsync(Guid idRegistro);
}
