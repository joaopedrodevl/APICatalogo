using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace APICatalogo.Models
{
    [Table("Categorias")]
    public class Categoria
    {
        [Key] // Mapeia a propriedade para a chave primária
        public int CategoriaId { get; set; }

        [Required]
        [StringLength(80)]
        public string? Nome { get; set; }

        [Required]
        [StringLength(300)]
        public string? ImageUrl { get; set; }

        [JsonIgnore]
        public ICollection<Produto>? Produtos { get; set; } // Relacionamento 1:N
    }
}
