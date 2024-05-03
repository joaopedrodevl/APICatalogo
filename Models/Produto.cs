using APICatalogo.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace APICatalogo.Models
{
    [Table("Produtos")]
    public class Produto : IValidatableObject // Interface para validação customizada
    {
        [Key] // Data Annotation que define a chave primária
        public int ProdutoId { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(80, ErrorMessage = "O nome deve ter entre 5 e 80 caracteres", MinimumLength = 5)]
        [PrimeiraLetraMaiuscula] // Custom Validation Attribute
        public string? Nome { get; set; }

        [Required]
        [StringLength(300, ErrorMessage = "A descrição deve ter no máximo 300 caracteres")]
        public string? Descricao { get; set; }

        [Required]
        [Range(1, 10000, ErrorMessage = "O preço deve estar entre 1 e 10000")]
        [Column(TypeName = "decimal(10, 2)")] // Define o tipo de dado no banco de dados
        public decimal Preco { get; set; }

        [Required] // Define que o campo é obrigatório
        [StringLength(300)] // Define o tamanho máximo da string
        public string? ImageUrl { get; set; }

        public float Estoque { get; set; }
        public DateTime DataCadastro { get; set; }

        public int CategoriaId { get; set; } // Chave estrangeira

        [JsonIgnore]
        public Categoria? Categoria { get; set; } // Propriedade de navegação, relacionamento 1:N

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(this.Nome))
            {
                var primeiraLetra = this.Nome[0].ToString();
                if (primeiraLetra != primeiraLetra.ToUpper())
                {
                    yield return new ValidationResult("A primeira letra do nome do produto deve ser maiúscula", new[] { nameof(Nome) });
                }
            } 

            if (this.Estoque <= 0)
            {
                yield return new ValidationResult("O estoque deve ser maior que zero", new[] { nameof(Estoque) });
            }   
        }
    }
}
