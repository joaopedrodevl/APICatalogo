using System.Text.Json;

namespace APICatalogo.Models
{
    public class ErrorDetails // Classe para detalhes de erro
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public string? Trace { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this); // Serializa o objeto ErrorDetails para JSON
        }
    }
}
