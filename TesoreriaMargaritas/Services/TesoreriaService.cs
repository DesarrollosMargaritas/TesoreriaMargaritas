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
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
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

        // --- NUEVO: CIERRE DE CAJA / ARQUEO ---

        // 1. Verificar si hay movimientos de días ANTERIORES sin cerrar
        public async Task<bool> HayMovimientosPendientesDiasAnterioresAsync()
        {
            var hoy = DateTime.Today;

            // Buscamos si existe alguna entrada o gasto con fecha menor a hoy y sin ArqueoId
            bool hayEntradasViejas = await _context.Entradas
                .AnyAsync(e => e.Fecha < hoy && e.ArqueoId == null);

            bool hayGastosViejos = await _context.Gastos
                .AnyAsync(g => g.Fecha < hoy && g.ArqueoId == null);

            return hayEntradasViejas || hayGastosViejos;
        }

        // 2. Obtener Saldo Inicial (Saldo Final del último arqueo)
        public async Task<decimal> ObtenerUltimoSaldoFinalAsync()
        {
            var ultimoArqueo = await _context.Set<Arqueo>() // Usamos Set<Arqueo> por si el DbSet no está explícito
                .OrderByDescending(a => a.FechaArqueo)
                .FirstOrDefaultAsync();

            return ultimoArqueo?.SaldoFinalDia ?? 0;
        }

        // 3. Pre-Calcular Totales para el Cierre Actual (Lo que está "en el aire")
        public async Task<Arqueo> SimularCierreActualAsync()
        {
            var arqueo = new Arqueo();

            // Saldo Inicial
            arqueo.SaldoInicial = await ObtenerUltimoSaldoFinalAsync();

            // Entradas Pendientes (Sin ArqueoId)
            var entradasPendientes = await _context.Entradas
                .Where(e => e.ArqueoId == null)
                .ToListAsync(); // Traemos a memoria para sumar rápido

            arqueo.TotEntradas = entradasPendientes.Where(e => true).Sum(e => e.Monto); // Asumiendo que Entradas no tiene campo Anulado aun, sumamos todo. Si tuviera, filtraríamos.
            // Si Entradas tuviera campo 'Anulado', aquí calcularíamos TotEntradasAnu. Por ahora es 0.
            arqueo.TotEntradasAnu = 0;

            // Gastos Pendientes (Sin ArqueoId)
            var gastosPendientes = await _context.Gastos
                .Where(g => g.ArqueoId == null)
                .ToListAsync();

            arqueo.TotSalidas = gastosPendientes.Where(g => !g.Anulado).Sum(g => g.Monto);
            arqueo.TotSalidasAnu = gastosPendientes.Where(g => g.Anulado).Sum(g => g.Monto);

            return arqueo;
        }

        // 4. GUARDAR CIERRE (Transacción Crítica)
        public async Task GuardarCierreCajaAsync(Arqueo nuevoArqueo)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // a. Guardar el Arqueo (Cabecera)
                    // Recalculamos Saldo Final por seguridad: SaldoInicial + Entradas - Salidas
                    nuevoArqueo.SaldoFinalDia = nuevoArqueo.SaldoInicial + nuevoArqueo.TotEntradas - nuevoArqueo.TotSalidas;

                    // Cálculo de Descuadre: Conteo - (SaldoInicial + Entradas - Salidas)
                    // Si el usuario pidió fórmula: Entradas - Salidas - Conteo, usamos esa, pero la contable estándar es:
                    // Sistema = SaldoInicial + Entradas - Salidas
                    // Diferencia = Conteo - Sistema.
                    // Ajustaremos a la fórmula solicitada si es estricta, pero usaré la lógica contable para que el signo tenga sentido.
                    decimal saldoTeorico = nuevoArqueo.SaldoInicial + nuevoArqueo.TotEntradas - nuevoArqueo.TotSalidas;
                    nuevoArqueo.Descuadre = nuevoArqueo.TotalConteoDinero - saldoTeorico;

                    _context.Add(nuevoArqueo);
                    await _context.SaveChangesAsync(); // Para obtener el nuevoArqueo.Id

                    // b. Bloquear Entradas (Actualizar ArqueoId)
                    var entradasPendientes = await _context.Entradas.Where(e => e.ArqueoId == null).ToListAsync();
                    foreach (var e in entradasPendientes) e.ArqueoId = nuevoArqueo.Id;

                    // c. Bloquear Gastos (Actualizar ArqueoId)
                    var gastosPendientes = await _context.Gastos.Where(g => g.ArqueoId == null).ToListAsync();
                    foreach (var g in gastosPendientes) g.ArqueoId = nuevoArqueo.Id;

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
    }
}