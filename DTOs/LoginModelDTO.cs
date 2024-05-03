using System.ComponentModel.DataAnnotations;

namespace APICatalogo.DTOs
{
    public class LoginModelDTO
    {
        [Required(ErrorMessage = "O User name é obrigatório.")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Password é obrigatório.")]
        public string? Password { get; set; }
    }
}
