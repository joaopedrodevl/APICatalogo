using APICatalogo.Models;
using APICatalogo.Pagination;
using X.PagedList;

namespace APICatalogo.Repositories
{
    public interface IProdutoRepository : IRepository<Produto>
    {
        //IQueryable<Produto> GetProdutos(); // IQueryable is used to query data from a database
        Task<IEnumerable<Produto>> GetProdutosPorCategoriaAsync(int categoriaId);
        //IEnumerable<Produto> GetProdutos(ProdutosParameters produtosParams);
        Task<IPagedList<Produto>> GetProdutosAsync(ProdutosParameters produtosParams);
        Task<IPagedList<Produto>> GetProdutosFiltroPrecoAsync(ProdutosFiltroPreco produtosFiltroPrecoParams);
    }
}
