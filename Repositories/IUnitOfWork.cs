namespace APICatalogo.Repositories
{
    // Deve abstrair, agrupar e persistir as operações de banco de dados.
    public interface IUnitOfWork 
    {
        IProdutoRepository ProdutoRepository { get; }
        ICategoriaRepository CategoriaRepository { get; }
        Task CommitAsync(); // Persiste as alterações no banco de dados.
    }
}
