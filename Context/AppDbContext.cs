using APICatalogo.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser> // Responsável por fazer a conexão com o banco de dados
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) // Construtor que recebe as opções de configuração do DbContext
        {
        }

        public DbSet<Categoria>? Categorias { get; set; } // DbSet é uma coleção de entidades que serão mapeadas para o banco de dados
        public DbSet<Produto>? Produtos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) // Método que é chamado quando o modelo é criado
        {
            base.OnModelCreating(builder); // Chama o método OnModelCreating da classe base
        }
    }
}
