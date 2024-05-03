using APICatalogo.Context;
using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Pagination;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace APICatalogo.Repositories
{
    public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
    {
        public CategoriaRepository(AppDbContext contexto) : base(contexto)
        {
        }

        public async Task<IPagedList<Categoria>> GetCategoriasAsync(CategoriasParameters categoriasParameters)
        {
            var categorias = await GetAllAsync();

            var categoriasOrdenadas = categorias.OrderBy(c => c.Nome).AsQueryable();

            // var resultado = PagedList<Categoria>.ToPagedList(categoriasOrdenadas, categoriasParameters.PageNumber, categoriasParameters.PageSize);

            var resultado = await categoriasOrdenadas.ToPagedListAsync(categoriasParameters.PageNumber, categoriasParameters.PageSize);

            return resultado;
        }

        public async Task<IPagedList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriasFiltroNome categoriasFiltroNomeParams)
        {
            var categorias = await GetAllAsync();

            if (!string.IsNullOrEmpty(categoriasFiltroNomeParams.Nome))
            {
                categorias = categorias.Where(c => c.Nome!.Contains(categoriasFiltroNomeParams.Nome));
            }

            // var categoriasFiltradas = PagedList<Categoria>.ToPagedList(categorias.AsQueryable(), categoriasFiltroNomeParams.PageNumber, categoriasFiltroNomeParams.PageSize);

            var categoriasFiltradas = await categorias.ToPagedListAsync(categoriasFiltroNomeParams.PageNumber, categoriasFiltroNomeParams.PageSize);

            return categoriasFiltradas;
        }

        //public IEnumerable<Categoria> GetCategorias()
        //{
        //    // Usar Where() para limitar a quantidade de registros retornados.
        //    var categorias = _context.Categorias.Include(x => x.Produtos).ToList(); // AsNoTracking() desabilita o rastreamento de entidades. Isso melhora o desempenho ao consultar entidades que não serão atualizadas no contexto atual. Deve ser usado quando a entidade não será alterada.

        //    if (categorias is null)
        //    {
        //        throw new InvalidOperationException("Categorias não encontradas.");
        //    }

        //    return categorias;
        //}

        //public Categoria GetCategoria(int categoriaId)
        //{
        //    return _context.Categorias.AsNoTracking().FirstOrDefault(c => c.CategoriaId == categoriaId); 
        //}

        //public Categoria Create(Categoria categoria)
        //{
        //    if (categoria is null)
        //    {
        //        throw new ArgumentNullException(nameof(categoria));
        //    }

        //    _context.Categorias.Add(categoria);
        //    _context.SaveChanges();
        //    return categoria;
        //}

        //public Categoria Update(Categoria categoria)
        //{
        //    if (categoria is null)
        //    {
        //        throw new ArgumentNullException(nameof(categoria));
        //    }

        //    _context.Entry(categoria).State = EntityState.Modified; // Marca a entidade como modificada. Isso significa que o Entity Framework Core executará uma atualização no banco de dados.
        //    _context.SaveChanges();
        //    return categoria;
        //}

        //public Categoria Delete(int id)
        //{
        //    var categoria = _context.Categorias.Find(id);

        //    if (categoria is null)
        //    {
        //        throw new InvalidOperationException($"Categoria com o id {id} não encontrada.");
        //    }

        //    _context.Categorias.Remove(categoria);
        //    _context.SaveChanges();
        //    return categoria;
        //}
    }
}
