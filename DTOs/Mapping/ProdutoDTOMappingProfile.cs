using APICatalogo.Models;
using AutoMapper;

namespace APICatalogo.DTOs.Mapping
{
    public class ProdutoDTOMappingProfile : Profile // Classe de mapeamento de ProdutoDTO AutoMapper
    {
        public ProdutoDTOMappingProfile()
        {
            CreateMap<Produto, ProdutoDTO>().ReverseMap();
            CreateMap<Categoria, CategoriaDTO>().ReverseMap();
            CreateMap<Produto, ProdutoDTOUpdateRequest>().ReverseMap();
            CreateMap<Produto, ProdutoDTOUpdateResponse>().ReverseMap();
        }
    }
}
