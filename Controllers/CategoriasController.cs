using APICatalogo.Context;
using APICatalogo.DTOs;
using APICatalogo.DTOs.Mapping;
using APICatalogo.Filters;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using APICatalogo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using X.PagedList;

namespace APICatalogo.Controllers
{
    [EnableCors("OrigensComAcessoPermitido")] // Habilita a pol�tica de CORS
    [Route("[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CategoriasController> _logger;

        public CategoriasController(IConfiguration configuration, ILogger<CategoriasController> logger, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [DisableCors] // Desabilita a pol�tica de CORS
        [HttpGet("LerArquivoConfiguracao")]
        public string GetValores()
        {
            _logger.LogInformation("########## GET /Categorias/LerArquivoConfiguracao ##########");

            var valor1 = _configuration["chave1"];
            var valor2 = _configuration["chave2"];
            var secao1 = _configuration["secao1:chave2"];

            return $"Valor1: {valor1} | Valor2: {valor2} | Secao1:Chave2: {secao1}";
        }

        [HttpGet("pagination")]
        public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get([FromQuery] CategoriasParameters categoriasParameters)
        {
            var categorias = await _unitOfWork.CategoriaRepository.GetCategoriasAsync(categoriasParameters);

            return ObterCategorias(categorias);
        }

        [HttpGet("filter/nome/pagination")]
        public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategoriasFiltroNome([FromQuery] CategoriasFiltroNome categoriasFiltroNomeParams)
        {
            var categorias = await _unitOfWork.CategoriaRepository.GetCategoriasFiltroNomeAsync(categoriasFiltroNomeParams);

            return ObterCategorias(categorias);
        }

        private ActionResult<IEnumerable<CategoriaDTO>> ObterCategorias(IPagedList<Categoria> categorias)
        {
            var metadata = new
            {
                categorias.Count,
                categorias.PageSize,
                categorias.PageCount,
                categorias.TotalItemCount,
                categorias.HasNextPage,
                categorias.HasPreviousPage
            };

            Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

            var categoriasDto = categorias.ToCategoriaDTOList();

            return Ok(categoriasDto);
        }

        [HttpGet]
        [ServiceFilter(typeof(ApiLoggingFilter))] // Adiciona o filtro de log de requisições
        [Authorize] // Adiciona a autorização
        public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategorias()
        {
            _logger.LogInformation("########## GET /Categorias/ ##########");
            
            var categorias = await _unitOfWork.CategoriaRepository.GetAllAsync();

            if (categorias is null)
            {
                return NotFound("Categorias não encontradas.");
            }

            var categoriasDto = categorias.ToCategoriaDTOList();

            return Ok(categoriasDto);
        }

        [DisableCors]
        [HttpGet("{id:int}", Name = "ObterCategoria")]
        public async Task<ActionResult<CategoriaDTO>> GetCategoria(int id)
        {
            _logger.LogInformation("########## GET /Categorias/id ##########");

            var categoria = await _unitOfWork.CategoriaRepository.GetAsync(c => c.CategoriaId == id);

            if (categoria is null)
            {
                return NotFound($"Categoria com o id {id} não encontrada.");
            }

            var categoriaDto = categoria.ToCategoriaDTO();

            return Ok(categoriaDto);
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaDTO>> PostCategoria(CategoriaDTO categoriaDto)
        {
            if (categoriaDto is null)
            {
                return BadRequest("Categoria inválida.");
            }

            var categoria = categoriaDto.ToCategoria();

            var categoriaCriada = _unitOfWork.CategoriaRepository.Create(categoria!);
            await _unitOfWork.CommitAsync();

            var novaCategoriaDto = categoriaCriada.ToCategoriaDTO();

            return new CreatedAtRouteResult("ObterCategoria", new { id = novaCategoriaDto?.CategoriaId }, novaCategoriaDto);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategoriaDTO>> PutCategoria(int id, CategoriaDTO categoriaDto)
        {
            if (id != categoriaDto.CategoriaId)
            {
                _logger.LogWarning("Categoria com o id não corresponde ao id da categoria.");
                return BadRequest("Dados inválidos");
            }

            var categoria = categoriaDto.ToCategoria();

            var categoriaAtualizada = _unitOfWork.CategoriaRepository.Update(categoria!);
            await _unitOfWork.CommitAsync();

            var categoriaAtualizadaDto = categoriaAtualizada?.ToCategoriaDTO();

            return Ok(categoriaAtualizadaDto);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<CategoriaDTO>> DeleteCategoria(int id)
        {
            var categoria = await _unitOfWork.CategoriaRepository.GetAsync(c => c.CategoriaId == id);

            if (categoria is null)
            {
                return NotFound($"Categoria com o id {id} não encontrada.");
            }

            var categoriaDeletada = _unitOfWork.CategoriaRepository.Delete(categoria);
            await _unitOfWork.CommitAsync();

            var categoriaDeletadaDto = categoriaDeletada.ToCategoriaDTO();

            return Ok(categoriaDeletadaDto);
        }
    }
}
