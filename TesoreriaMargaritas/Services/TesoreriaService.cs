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

        // --- ENTRADAS ---
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

        // --- SALIDAS / GASTOS ---

        // Nuevo método para el Dashboard
        public async Task<decimal> ObtenerTotalGastosHoyAsync()
        {
            var hoy = DateTime.Today;
            return await _context.Gastos
                .Where(g => g.Fecha >= hoy && !g.Anulado)
                .SumAsync(g => g.Monto);
        }

        public async Task<int> ObtenerSiguienteConsecutivoAsync(string prefijo)
        {
            var secuencia = await _context.SecuenciasPrefijos.FindAsync(prefijo);
            if (secuencia == null) return 1;
            return secuencia.UltimoConsecutivo + 1;
        }

        public async Task RegistrarGastoAsync(Gasto gasto)
        {
            // CORRECCIÓN: Usar ExecutionStrategy para soportar EnableRetryOnFailure
            // Esto evita el error: 'SqlServerRetryingExecutionStrategy does not support user-initiated transactions'
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Si NO es Factura (consecutivo manual), calculamos el automático
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

                    _context.Gastos.Add(gasto);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
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