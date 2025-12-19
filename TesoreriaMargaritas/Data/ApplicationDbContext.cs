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
        
        // --- Módulo Salidas ---
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<SecuenciaPrefijo> SecuenciasPrefijos { get; set; }
        
        // Tablas ERP (Solo Lectura idealmente)
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Vendedor> Vendedores { get; set; }
    }
}