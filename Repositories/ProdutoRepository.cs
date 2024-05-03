using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using X.PagedList;

namespace APICatalogo.Repositories
{
    public class ProdutoRepository : Repository<Produto>, IProdutoRepository
    {

        public ProdutoRepository(AppDbContext contexto) : base(contexto)
        {
        }

        public async Task<IPagedList<Produto>> GetProdutosAsync(ProdutosParameters produtosParams)
        {
            var produtos = await GetAllAsync();
            
            var produtosOrdenados = produtos.OrderBy(p => p.ProdutoId).AsQueryable();

            // var resultado = PagedList<Produto>.ToPagedList(produtosOrdenados, produtosParams.PageNumber, produtosParams.PageSize);

            var resultado = await produtosOrdenados.ToPagedListAsync(produtosParams.PageNumber, produtosParams.PageSize);
        
            return resultado;
        }

        public async Task<IPagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroPrecoParams)
        {
            var produtos = await GetAllAsync();

            if (produtosFiltroPrecoParams.Preco.HasValue && !string.IsNullOrEmpty(produtosFiltroPrecoParams.PrecoCriterio))
            {
                produtos = produtosFiltroPrecoParams.PrecoCriterio switch
                {
                    "maior" => produtos.Where(p => p.Preco > produtosFiltroPrecoParams.Preco),
                    "menor" => produtos.Where(p => p.Preco < produtosFiltroPrecoParams.Preco),
                    "igual" => produtos.Where(p => p.Preco == produtosFiltroPrecoParams.Preco),
                    _ => produtos
                };
            }

            // var produtosOrdenados = PagedList<Produto>.ToPagedList(produtos.AsQueryable(), produtosFiltroPrecoParams.PageNumber, produtosFiltroPrecoParams.PageSize);

            var produtosOrdenados = await produtos.ToPagedListAsync(produtosFiltroPrecoParams.PageNumber, produtosFiltroPrecoParams.PageSize);

            return produtosOrdenados;
        }

        //public IEnumerable<Produto> GetProdutos(ProdutosParameters produtosParams)
        //{
        //    return GetAll()
        //        .OrderBy(on => on.Nome) // Ordena os produtos pelo nome.
        //        .Skip((produtosParams.PageNumber - 1) * produtosParams.PageSize) // Pula os registros de acordo com a página atual e a quantidade de registros por página.
        //        .Take(produtosParams.PageSize); // Retorna a quantidade de registros de acordo com a quantidade de registros por página.
        //}

        public async Task<IEnumerable<Produto>> GetProdutosPorCategoriaAsync(int categoriaId)
        {
            var produtos = await GetAllAsync();

            return produtos.Where(p => p.CategoriaId == categoriaId);
        }   

        //public Produto GetProduto(int id)
        //{
        //    var produto = _context.Produtos.FirstOrDefault(p => p.ProdutoId == id); // Encontra o produto pelo id.

        //    if (produto is null)
        //    {
        //        throw new InvalidOperationException("Produto não encontrado.");
        //    }

        //    return produto;
        //}

        //// IQueryable é muito usado para consultas de banco de dados, pois permite a execução de consultas de forma eficiente e ordenada.
        //public IQueryable<Produto> GetProdutos() // Retorna uma consulta de produtos. IQueryable é uma interface que permite a iteração de uma coleção de itens. Lazy loading.
        //{
        //    // Sempre usar Take() para limitar a quantidade de registros retornados.
        //    return _context.Produtos;
        //}

        //public Produto Create(Produto produto)
        //{
        //    if (produto is null)
        //    {
        //        throw new InvalidOperationException("Produto é null");
        //    }

        //    _context.Produtos.Add(produto);
        //    _context.SaveChanges();
        //    return produto;
        //}
        //public bool Update(Produto produto)
        //{
        //    if (produto is null)
        //    {
        //        throw new InvalidOperationException("Produto é null");
        //    }

        //    if (_context.Produtos.Any(p => p.ProdutoId == produto.ProdutoId)) // Verifica se o produto existe. Método LINQ.
        //    {
        //        _context.Produtos.Update(produto); // Atualiza o produto. Update() vs Entry().State = EntityState.Modified. Update() é mais simples e direto. Entry().State = EntityState.Modified é mais flexível.
        //        _context.SaveChanges();
        //        return true;
        //    }

        //    return false;
        //}

        //public bool Delete(int id)
        //{
        //    var produto = _context.Produtos.Find(id); // Encontra o produto pelo id.

        //    if (produto is not null)
        //    {
        //        _context.Produtos.Remove(produto); // Remove o produto.
        //        _context.SaveChanges();
        //        return true;
        //    }

        //    return false;
        //}
    }
}
