using Microsoft.EntityFrameworkCore;
using TesoreriaMargaritas.Data;
using TesoreriaMargaritas.Models;
using System.Text.Json;
using ClosedXML.Excel;

namespace TesoreriaMargaritas.Services
{
    public class TransaccionDTO
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = "";
        public string Referencia { get; set; } = "";
        public string Concepto { get; set; } = "";
        public string Detalle { get; set; } = "";
        public string FormaPago { get; set; } = "";
        public decimal Monto { get; set; }
        public string Usuario { get; set; } = "";
        public bool Anulado { get; set; }
        public string Estado => Anulado ? "ANULADO" : "Exitoso";
    }

    public class ContadorKPI
    {
        public decimal IngresosMes { get; set; }
        public decimal GastosMes { get; set; }
        public decimal SaldoCaja { get; set; }
        public int PendientesCierre { get; set; }
    }

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
            return await _context.Entradas.Include(e => e.Usuario).OrderByDescending(e => e.Fecha).ToListAsync();
        }

        public async Task<decimal> ObtenerTotalEntradasHoyAsync()
        {
            return await _context.Entradas.Where(e => e.ArqueoId == null && !e.Anulado).SumAsync(e => e.Monto);
        }

        public async Task RegistrarEntradaAsync(Entrada entrada)
        {
            _context.Entradas.Add(entrada);
            await _context.SaveChangesAsync();
        }

        // --- SALIDAS / GASTOS ---
        public async Task<List<Proveedor>> BuscarProveedoresAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino)) return new List<Proveedor>();
            return await _context.Proveedores.Where(p => p.NOMPROVEEDOR.Contains(termino)).Take(20).ToListAsync();
        }

        public async Task<List<Proveedor>> ObtenerTodosProveedoresAsync() => await _context.Proveedores.Take(1000).ToListAsync();
        public async Task<List<Vendedor>> ObtenerVendedoresAsync() => await _context.Vendedores.ToListAsync();

        public async Task<decimal> ObtenerTotalGastosHoyAsync()
        {
            return await _context.Gastos.Where(g => g.ArqueoId == null && !g.Anulado).SumAsync(g => g.Monto);
        }

        public async Task<int> ObtenerSiguienteConsecutivoAsync(string prefijo)
        {
            var secuencia = await _context.SecuenciasPrefijos.FindAsync(prefijo);
            return secuencia == null ? 1 : secuencia.UltimoConsecutivo + 1;
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
                catch { await transaction.RollbackAsync(); throw; }
            });
        }

        public async Task<List<Gasto>> ObtenerGastosAsync()
        {
            return await _context.Gastos.Include(g => g.Usuario).OrderByDescending(g => g.Fecha).ToListAsync();
        }

        // --- CIERRE DE CAJA ---
        public async Task<bool> HayMovimientosPendientesDiasAnterioresAsync()
        {
            var hoy = DateTime.Today;
            return await _context.Entradas.AnyAsync(e => e.Fecha < hoy && e.ArqueoId == null) ||
                   await _context.Gastos.AnyAsync(g => g.Fecha < hoy && g.ArqueoId == null);
        }

        public async Task<Arqueo> SimularCierreActualAsync()
        {
            var arqueo = new Arqueo();

            // 1. SALDOS INICIALES (CONTINUIDAD DE CAJA)
            var ultimoArqueo = await _context.Arqueos
                                    .OrderByDescending(a => a.FechaArqueo)
                                    .ThenByDescending(a => a.FechaHora)
                                    .FirstOrDefaultAsync();

            if (ultimoArqueo != null)
            {
                // Efectivo: Inicia con lo que había físico ayer
                arqueo.SaldoInicialEfectivo = ultimoArqueo.FisicoEfectivo;
                arqueo.SaldoArrastreAnterior = ultimoArqueo.DescuadreEfectivo;

                // Digitales: Inicia con lo que se reportó en la App ayer
                arqueo.SaldoInicialNequi = ultimoArqueo.ReportadoNequi;
                arqueo.SaldoInicialDaviplata = ultimoArqueo.ReportadoDaviplata;
            }
            else
            {
                // Primer arqueo de la historia
                arqueo.SaldoInicialEfectivo = 0;
                arqueo.SaldoArrastreAnterior = 0;
                arqueo.SaldoInicialNequi = 0;
                arqueo.SaldoInicialDaviplata = 0;
            }

            // 2. CLASIFICAR ENTRADAS
            var entradas = await _context.Entradas.Where(e => e.ArqueoId == null && !e.Anulado).ToListAsync();
            arqueo.SistEntradasEfectivo = entradas.Where(e => e.FormaPago == "Efectivo").Sum(e => e.Monto);
            arqueo.SistEntradasNequi = entradas.Where(e => e.FormaPago == "Nequi").Sum(e => e.Monto);
            arqueo.SistEntradasDaviplata = entradas.Where(e => e.FormaPago == "Daviplata").Sum(e => e.Monto);

            // 3. CLASIFICAR SALIDAS
            var gastos = await _context.Gastos.Where(g => g.ArqueoId == null && !g.Anulado).ToListAsync();
            arqueo.SistSalidasEfectivo = gastos.Where(g => g.FormaPago == "Efectivo").Sum(g => g.Monto);
            arqueo.SistSalidasNequi = gastos.Where(g => g.FormaPago == "Nequi").Sum(g => g.Monto);
            arqueo.SistSalidasDaviplata = gastos.Where(g => g.FormaPago == "Daviplata").Sum(g => g.Monto);

            // 4. CALCULAR TOTALES ESPERADOS (Ahora todos incluyen Saldo Inicial)

            // Efectivo
            arqueo.SistTotalEfectivo = arqueo.SaldoInicialEfectivo + arqueo.SistEntradasEfectivo - arqueo.SistSalidasEfectivo;

            // Nequi
            arqueo.SistTotalNequi = arqueo.SaldoInicialNequi + arqueo.SistEntradasNequi - arqueo.SistSalidasNequi;

            // Daviplata
            arqueo.SistTotalDaviplata = arqueo.SaldoInicialDaviplata + arqueo.SistEntradasDaviplata - arqueo.SistSalidasDaviplata;

            return arqueo;
        }

        public async Task GuardarCierreCajaAsync(Arqueo nuevoArqueo)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Recalcular todo para asegurar consistencia al guardar

                    // Efectivo
                    nuevoArqueo.SistTotalEfectivo = nuevoArqueo.SaldoInicialEfectivo + nuevoArqueo.SistEntradasEfectivo - nuevoArqueo.SistSalidasEfectivo;
                    nuevoArqueo.DescuadreEfectivo = nuevoArqueo.FisicoEfectivo - nuevoArqueo.SistTotalEfectivo;

                    // Nequi
                    nuevoArqueo.SistTotalNequi = nuevoArqueo.SaldoInicialNequi + nuevoArqueo.SistEntradasNequi - nuevoArqueo.SistSalidasNequi;
                    nuevoArqueo.DescuadreNequi = nuevoArqueo.ReportadoNequi - nuevoArqueo.SistTotalNequi;

                    // Daviplata
                    nuevoArqueo.SistTotalDaviplata = nuevoArqueo.SaldoInicialDaviplata + nuevoArqueo.SistEntradasDaviplata - nuevoArqueo.SistSalidasDaviplata;
                    nuevoArqueo.DescuadreDaviplata = nuevoArqueo.ReportadoDaviplata - nuevoArqueo.SistTotalDaviplata;

                    _context.Arqueos.Add(nuevoArqueo);
                    await _context.SaveChangesAsync();

                    // Marcar transacciones
                    var entradas = await _context.Entradas.Where(e => e.ArqueoId == null).ToListAsync();
                    foreach (var e in entradas) e.ArqueoId = nuevoArqueo.Id;

                    var gastos = await _context.Gastos.Where(g => g.ArqueoId == null).ToListAsync();
                    foreach (var g in gastos) g.ArqueoId = nuevoArqueo.Id;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch { await transaction.RollbackAsync(); throw; }
            });
        }

        // --- AUXILIARES Y REPORTES ---
        public async Task<int> ContarEntradasPendientesAsync() => await _context.Entradas.CountAsync(e => e.ArqueoId == null);
        public async Task<int> ContarGastosPendientesAsync() => await _context.Gastos.CountAsync(g => g.ArqueoId == null);

        public async Task<Dictionary<string, decimal>> ObtenerResumenEntradasPorConceptoAsync()
        {
            return await _context.Entradas.Where(e => e.ArqueoId == null && !e.Anulado)
               .GroupBy(e => e.Concepto).Select(g => new { K = g.Key, V = g.Sum(e => e.Monto) })
               .ToDictionaryAsync(x => x.K, x => x.V);
        }
        public async Task<Dictionary<string, decimal>> ObtenerResumenGastosPorConceptoAsync()
        {
            return await _context.Gastos.Where(g => g.ArqueoId == null && !g.Anulado)
               .GroupBy(g => g.Concepto).Select(g => new { K = g.Key, V = g.Sum(g => g.Monto) })
               .ToDictionaryAsync(x => x.K, x => x.V);
        }

        public async Task<List<Arqueo>> ObtenerHistorialCierresAsync() => await _context.Arqueos.Include(a => a.Usuario).OrderByDescending(a => a.FechaArqueo).Take(50).ToListAsync();

        public async Task<List<TransaccionDTO>> ObtenerMovimientosPorRangoAsync(DateTime inicio, DateTime fin)
        {
            var fechaFin = fin.Date.AddDays(1).AddTicks(-1);
            var movimientos = new List<TransaccionDTO>();

            var entradas = await _context.Entradas.Include(e => e.Usuario).Where(e => e.Fecha >= inicio.Date && e.Fecha <= fechaFin).ToListAsync();
            var gastos = await _context.Gastos.Include(g => g.Usuario).Where(g => g.Fecha >= inicio.Date && g.Fecha <= fechaFin).ToListAsync();

            foreach (var e in entradas) movimientos.Add(new TransaccionDTO
            {
                Id = e.Id,
                Fecha = e.Fecha,
                Tipo = "Entrada",
                Referencia = $"ENT-{e.Id}",
                Concepto = e.Concepto,
                Detalle = "Ingreso",
                Monto = e.Monto,
                FormaPago = e.FormaPago,
                Usuario = e.UsuarioId,
                Anulado = e.Anulado
            });

            foreach (var g in gastos) movimientos.Add(new TransaccionDTO
            {
                Id = g.Id,
                Fecha = g.Fecha,
                Tipo = "Salida",
                Referencia = $"{g.Prefijo}-{g.Consecutivo}",
                Concepto = g.Concepto,
                Detalle = g.Beneficiario,
                Monto = g.Monto,
                FormaPago = g.FormaPago,
                Usuario = g.UsuarioId,
                Anulado = g.Anulado
            });

            return movimientos.OrderByDescending(m => m.Fecha).ToList();
        }

        public async Task<byte[]> GenerarReporteExcelMovimientosAsync(DateTime inicio, DateTime fin)
        {
            var movimientos = await ObtenerMovimientosPorRangoAsync(inicio, fin);
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Movimientos");
            ws.Cell("A1").Value = "REPORTE";
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream.ToArray();
        }

        public async Task<byte[]> GenerarReporteExcelCierreAsync(int id)
        {
            return Array.Empty<byte>();
        }

        public async Task<List<TransaccionDTO>> ObtenerMovimientosPendientesAsync()
        {
            var movimientos = await ObtenerMovimientosPorRangoAsync(DateTime.MinValue, DateTime.MaxValue);
            return movimientos.Where(m => m.Estado != "Cerrado" && !m.Anulado).ToList();
        }

        public async Task AnularTransaccionAsync(int id, string tipo)
        {
            if (tipo == "Entrada")
            {
                var ent = await _context.Entradas.FindAsync(id);
                if (ent != null && ent.ArqueoId == null) { ent.Anulado = true; await _context.SaveChangesAsync(); }
            }
            else
            {
                var gasto = await _context.Gastos.FindAsync(id);
                if (gasto != null && gasto.ArqueoId == null) { gasto.Anulado = true; await _context.SaveChangesAsync(); }
            }
        }

        public async Task ModificarMontoTransaccionAsync(int id, string tipo, decimal monto)
        {
            if (tipo == "Entrada")
            {
                var ent = await _context.Entradas.FindAsync(id);
                if (ent != null && ent.ArqueoId == null) { ent.Monto = monto; await _context.SaveChangesAsync(); }
            }
            else
            {
                var gasto = await _context.Gastos.FindAsync(id);
                if (gasto != null && gasto.ArqueoId == null) { gasto.Monto = monto; await _context.SaveChangesAsync(); }
            }
        }

        public async Task<ContadorKPI> ObtenerKPIsContadorAsync()
        {
            return new ContadorKPI();
        }
    }
}