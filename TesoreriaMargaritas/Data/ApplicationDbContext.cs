using Microsoft.EntityFrameworkCore;
using TesoreriaMargaritas.Models;

namespace TesoreriaMargaritas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Entrada> Entradas { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<SecuenciaPrefijo> SecuenciasPrefijos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Vendedor> Vendedores { get; set; }

        // --- CORRECCIÓN: Agregamos el DbSet que faltaba para que funcione el Cierre ---
        public DbSet<Arqueo> Arqueos { get; set; }
    }
}