using APICatalogo.Context;
using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using X.PagedList;

namespace APICatalogo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper; // Mapeia um objeto de um tipo para outro.

        public ProdutosController(IUnitOfWork unitOfWork, IMapper mapper)
        {

            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("pagination")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get([FromQuery] ProdutosParameters produtosParameters)
        {
            var produtos = await _unitOfWork.ProdutoRepository.GetProdutosAsync(produtosParameters);

            return ObterProdutos(produtos);
        }

        [HttpGet("filter/preco/pagination")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosFilterPreco([FromQuery] ProdutosFiltroPreco produtosFiltroPreco)
        {
            var produtos = await _unitOfWork.ProdutoRepository.GetProdutosFiltroPrecoAsync(produtosFiltroPreco);

            return ObterProdutos(produtos);
        }

        private ActionResult<IEnumerable<ProdutoDTO>> ObterProdutos(IPagedList<Produto> produtos)
        {
            var metadata = new
            {
                produtos.Count,
                produtos.PageSize,
                produtos.PageCount,
                produtos.TotalItemCount,
                produtos.HasNextPage,
                produtos.HasPreviousPage
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

            return Ok(produtosDto);
        }

        [Authorize(Policy ="UserOnly")]
        [HttpGet] // Decorador que indica que o método responde a requisições HTTP GET.
        // Método action é um método de uma classe de controlador que responde a uma solicitação HTTP.
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutos() // Retorna todos os produtos. IEnumerable é uma interface que permite a iteração de uma coleção de itens.
        {
            var produtos = await _unitOfWork.ProdutoRepository.GetAllAsync(); 

            if (produtos is null)
            {
                return NotFound("Produtos não encontrados.");
            }

            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);
            return Ok(produtosDto);
        }

        [HttpGet("{id:int:min(1)}", Name = "ObterProduto")] // Decorador que indica que o método responde a requisições HTTP GET com um parâmetro de rota.
        public async Task<ActionResult<ProdutoDTO>> GetProduto(int id) // Retorna um produto específico.
        {
            var produto = await _unitOfWork.ProdutoRepository.GetAsync(c => c.ProdutoId == id); // Retorna o produto com o id especificado.

            if (produto is null)
            {
                return NotFound($"Produto com o id {id} não encontrado.");
            }

            var produtosDto = _mapper.Map<ProdutoDTO>(produto);
            return Ok(produtosDto);
        }

        [HttpGet("produtos/{id:int:min(1)}")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosPorCategoria(int id) // Retorna todos os produtos de uma categoria específica.
        {
            var produtos = _unitOfWork.ProdutoRepository.GetProdutosPorCategoriaAsync(id);

            if (produtos is null)
            {
                return NotFound($"Produtos da categoria com o id {id} não encontrados.");
            }

            // var destino = _mapper.Map<Destino>(Origem);
            var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos); // Mapeia os produtos para o DTO (Data Transfer Object).

            return Ok(produtosDto);
        }

        [HttpPost] // Decorador que indica que o método responde a requisições HTTP POST.
        public async Task<ActionResult<ProdutoDTO>> PostProduto(ProdutoDTO produtoDto) // Adiciona um novo produto.
        {
            if (produtoDto is null)
            {
                return BadRequest("Produto inválido.");
            }

            var produto = _mapper.Map<Produto>(produtoDto); // Mapeia o DTO para o objeto Produto.

            var produtoCriado = _unitOfWork.ProdutoRepository.Create(produto);
            await _unitOfWork.CommitAsync();

            var produtoDtoCriado = _mapper.Map<ProdutoDTO>(produtoCriado); // Mapeia o objeto Produto para o DTO.
            return new CreatedAtRouteResult("ObterProduto", new { id = produtoDtoCriado.ProdutoId }, produtoDtoCriado); // Retorna um código de status 201 (Created) e um cabeçalho Location com o URI do novo recurso.
        }

        [HttpPatch("{id:int:min(1)}/UpdatePartial")]
        public async Task<ActionResult<ProdutoDTOUpdateResponse>> Patch(int id, JsonPatchDocument<ProdutoDTOUpdateRequest> patchProdutoDto)
        {
            if (patchProdutoDto is null || id <= 0)
            {
                return BadRequest();
            }

            var produto = await _unitOfWork.ProdutoRepository.GetAsync(c => c.ProdutoId == id);

            if (produto is null)
            {
                return NotFound();
            }

            var produtoUpdateRequest = _mapper.Map<ProdutoDTOUpdateRequest>(produto); // Mapeia o objeto Produto para o DTO de atualização.

            patchProdutoDto.ApplyTo(produtoUpdateRequest, ModelState); // Aplica as alterações do DTO de atualização ao DTO.

            if (!ModelState.IsValid || TryValidateModel(produtoUpdateRequest))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(produtoUpdateRequest, produto); // Mapeia o DTO de atualização para o objeto Produto.

            _unitOfWork.ProdutoRepository.Update(produto);
            await _unitOfWork.CommitAsync();

            return Ok(_mapper.Map<ProdutoDTOUpdateResponse>(produto)); // Mapeia o objeto Produto para o DTO de resposta.
        }

        [HttpPut("{id:int}")] // Decorador que indica que o método responde a requisições HTTP PUT com um parâmetro de rota.
        public async Task<ActionResult<ProdutoDTO>> PutProduto(int id, ProdutoDTO produtoDto) // Atualiza um produto.
        {
            if (id != produtoDto.ProdutoId)
            {
                return BadRequest($"Erro ao encontrar produto com ID={id}");
            }

            var produto = _mapper.Map<Produto>(produtoDto);

            var produtoAtualizado = _unitOfWork.ProdutoRepository.Update(produto);
            await _unitOfWork.CommitAsync();

            var produtoDtoAtualizado = _mapper.Map<ProdutoDTO>(produtoAtualizado);

            return Ok(new {
                sucesso = true,
                message = $"Produto com o id {id} foi atualizado.",
                produto = produtoDtoAtualizado
            });
        }

        [HttpDelete("{id:int}")] // Decorador que indica que o método responde a requisições HTTP DELETE com um parâmetro de rota.
        public async Task<ActionResult<ProdutoDTO>> DeleteProduto(int id) // Deleta um produto.
        {
            var produto = await _unitOfWork.ProdutoRepository.GetAsync(c => c.ProdutoId == id);

            if (produto is null)
            {
                return NotFound($"Produto com o id {id} não encontrado.");
            }

            var produtoDeletado = _unitOfWork.ProdutoRepository.Delete(produto);
            await _unitOfWork.CommitAsync();

            var produtoDtoDeletado = _mapper.Map<ProdutoDTO>(produtoDeletado);

            return Ok(new {
                sucesso = true,
                message = $"Produto com o id {id} foi deletado.",
                produto = produtoDtoDeletado
            });
        }   
    }
}
