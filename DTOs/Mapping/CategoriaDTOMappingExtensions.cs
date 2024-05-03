using APICatalogo.Models;

namespace APICatalogo.DTOs.Mapping
{
    public static class CategoriaDTOMappingExtensions
    {
        public static CategoriaDTO? ToCategoriaDTO(this Categoria categoria) // O this é um modificador de parâmetro que indica que o método de extensão é uma extensão para o tipo de dados especificado. Neste caso, o método ToCategoriaDTO é uma extensão para o tipo Categoria. Ou seja, o método ToCategoriaDTO é um método de extensão para o tipo Categoria.
        {
            return categoria is null ? null : new CategoriaDTO
            {
                CategoriaId = categoria.CategoriaId,
                Nome = categoria.Nome,
                ImageUrl = categoria.ImageUrl
            };
        }

        public static Categoria? ToCategoria(this CategoriaDTO categoriaDTO)
        {
            return categoriaDTO is null ? null : new Categoria
            {
                CategoriaId = categoriaDTO.CategoriaId,
                Nome = categoriaDTO.Nome,
                ImageUrl = categoriaDTO.ImageUrl
            };
        }

        public static IEnumerable<CategoriaDTO> ToCategoriaDTOList(this IEnumerable<Categoria> categorias)
        {
            if (categorias is null || !categorias.Any())
            {
                return new List<CategoriaDTO>(); // Retorna uma lista vazia de CategoriaDTO.
            }

            return categorias.Select(categoria => categoria.ToCategoriaDTO()!).ToList(); // O método Select é um método de extensão do namespace System.Linq que é usado para projetar cada elemento de uma sequência em um novo formulário. Neste caso, o método Select é usado para projetar cada elemento da sequência de categorias em um novo formulário de CategoriaDTO.
        }
    }
}
