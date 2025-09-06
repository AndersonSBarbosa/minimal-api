using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.Entidades;

namespace minimal_api.Dominio.Infraestrutura.DB
{
    public class DbContexto : DbContext
    { 
        private readonly IConfiguration _configuration;
        public DbContexto(IConfiguration configuration)
        {
            _configuration=configuration;
        }
        public DbSet<Administrador> Administradores { get; set; } = default!;
        public DbSet<Veiculo> Veiculos { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var StringConexao = _configuration.GetConnectionString("ManagerAPISqlServer");
            optionsBuilder.UseSqlServer(StringConexao);
        }
    }
}
