using Microsoft.EntityFrameworkCore;
using TesoreriaMargaritas.Data;
using TesoreriaMargaritas.Models;

namespace TesoreriaMargaritas.Services
{
    public class TesoreriaService
    {
        private readonly ApplicationDbContext _context;

        public TesoreriaService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- ENTRADAS (Existente) ---
        public async Task<List<Entrada>> ObtenerEntradasAsync()
        {
            return await _context.Entradas
                .Include(e => e.Usuario)
                .OrderByDescending(e => e.Fecha)
                .ToListAsync();
        }

        public async Task<decimal> ObtenerTotalEntradasHoyAsync()
        {
            var hoy = DateTime.Today;
            return await _context.Entradas
                .Where(e => e.Fecha >= hoy)
                .SumAsync(e => e.Monto);
        }

        public async Task RegistrarEntradaAsync(Entrada entrada)
        {
            _context.Entradas.Add(entrada);
            await _context.SaveChangesAsync();
        }

        // --- NUEVO: SALIDAS / GASTOS ---

        // 1. Listas para Dropdowns (Optimizadas para búsqueda rápida)
        public async Task<List<Proveedor>> BuscarProveedoresAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino)) return new List<Proveedor>();
            
            return await _context.Proveedores
                .Where(p => p.NOMPROVEEDOR.Contains(termino))
                .Take(20) // Limitamos para no sobrecargar
                .ToListAsync();
        }
        
        public async Task<List<Proveedor>> ObtenerTodosProveedoresAsync() 
        {
             // Ojo: Si son miles, esto puede ser lento. Mejor usar datalist en el cliente
             return await _context.Proveedores.Take(1000).ToListAsync();
        }

        public async Task<List<Vendedor>> ObtenerVendedoresAsync()
        {
            return await _context.Vendedores.ToListAsync();
        }

        // 2. Lógica de Consecutivo Automático
        public async Task<int> ObtenerSiguienteConsecutivoAsync(string prefijo)
        {
            var secuencia = await _context.SecuenciasPrefijos.FindAsync(prefijo);
            if (secuencia == null)
            {
                return 1; // Si no existe, arranca en 1
            }
            return secuencia.UltimoConsecutivo + 1;
        }

        // 3. Registrar Gasto (Transaccional)
        public async Task RegistrarGastoAsync(Gasto gasto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Si NO es Factura, calculamos y actualizamos el consecutivo automático
                if (gasto.Concepto != "Facturas")
                {
                    var secuencia = await _context.SecuenciasPrefijos.FindAsync(gasto.Prefijo);
                    if (secuencia == null)
                    {
                        secuencia = new SecuenciaPrefijo { Prefijo = gasto.Prefijo, UltimoConsecutivo = 0 };
                        _context.SecuenciasPrefijos.Add(secuencia);
                    }
                    
                    secuencia.UltimoConsecutivo++;
                    gasto.Consecutivo = secuencia.UltimoConsecutivo;
                }

                // Guardamos el gasto
                _context.Gastos.Add(gasto);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Gasto>> ObtenerGastosAsync()
        {
            return await _context.Gastos
                .Include(g => g.Usuario)
                .OrderByDescending(g => g.Fecha)
                .ToListAsync();
        }
    }
}