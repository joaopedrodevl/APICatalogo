using APICatalogo.Context;

namespace APICatalogo.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private IProdutoRepository? _produtoRepository; // Esses repositórios são necessários para a implementação do padrão Unit of Work, servem para acessar os métodos de CRUD.

        private ICategoriaRepository? _categoriaRepository;
        public AppDbContext _context;

        public UnitOfWork(AppDbContext contexto)
        {
            _context = contexto;
        }

        // Lazy loading: a instância do repositório só é criada quando é chamada.
        public IProdutoRepository ProdutoRepository
        {
            get
            {
                return _produtoRepository = _produtoRepository ?? new ProdutoRepository(_context); // Se _produtoRepository for nulo, cria uma nova instância de ProdutoRepository
            }
        }

        public ICategoriaRepository CategoriaRepository
        {
            get
            {
                return _categoriaRepository = _categoriaRepository ?? new CategoriaRepository(_context);
            }
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose() // Responsável por liberar recursos não gerenciados.
        {
            _context.Dispose();
        }
    }
}
