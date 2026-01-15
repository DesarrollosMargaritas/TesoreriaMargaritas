using Microsoft.EntityFrameworkCore;
using TesoreriaMargaritas.Data;
using TesoreriaMargaritas.Models;
using System.Text.Json;
using ClosedXML.Excel;

namespace TesoreriaMargaritas.Services
{
    // DTO para unificar Entradas y Gastos en una sola lista
    public class TransaccionDTO
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = ""; // "Entrada" o "Salida"
        public string Referencia { get; set; } = "";
        public string Concepto { get; set; } = "";
        public string Detalle { get; set; } = ""; // Beneficiario o Usuario
        public decimal Monto { get; set; }
        public string Usuario { get; set; } = "";
        public bool Anulado { get; set; }
        public string Estado => Anulado ? "ANULADO" : "Exitoso";
    }

    // DTO para KPIs del Contador
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
            var hoy = DateTime.Today;
            // Solo sumamos las NO anuladas
            return await _context.Entradas.Where(e => e.Fecha >= hoy && !e.Anulado).SumAsync(e => e.Monto);
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

        public async Task<List<Proveedor>> ObtenerTodosProveedoresAsync()
        {
            return await _context.Proveedores.Take(1000).ToListAsync();
        }

        public async Task<List<Vendedor>> ObtenerVendedoresAsync()
        {
            return await _context.Vendedores.ToListAsync();
        }

        public async Task<decimal> ObtenerTotalGastosHoyAsync()
        {
            var hoy = DateTime.Today;
            return await _context.Gastos.Where(g => g.Fecha >= hoy && !g.Anulado).SumAsync(g => g.Monto);
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

        public async Task<decimal> ObtenerUltimoSaldoFinalAsync()
        {
            var ultimo = await _context.Set<Arqueo>().OrderByDescending(a => a.FechaArqueo).FirstOrDefaultAsync();
            return ultimo?.SaldoFinalDia ?? 0;
        }

        public async Task<Arqueo> SimularCierreActualAsync()
        {
            var arqueo = new Arqueo();

            // --- CAMBIO APLICADO: DÍAS INDEPENDIENTES ---
            // Antes: arqueo.SaldoInicial = await ObtenerUltimoSaldoFinalAsync();
            // Ahora:
            arqueo.SaldoInicial = 0;
            // --------------------------------------------

            var entradas = await _context.Entradas.Where(e => e.ArqueoId == null).ToListAsync();
            arqueo.TotEntradas = entradas.Where(e => !e.Anulado).Sum(e => e.Monto);
            arqueo.TotEntradasAnu = entradas.Where(e => e.Anulado).Sum(e => e.Monto);

            var gastos = await _context.Gastos.Where(g => g.ArqueoId == null).ToListAsync();
            arqueo.TotSalidas = gastos.Where(g => !g.Anulado).Sum(g => g.Monto);
            arqueo.TotSalidasAnu = gastos.Where(g => g.Anulado).Sum(g => g.Monto);

            return arqueo;
        }

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

        public async Task<int> ContarEntradasPendientesAsync() => await _context.Entradas.CountAsync(e => e.ArqueoId == null);
        public async Task<int> ContarGastosPendientesAsync() => await _context.Gastos.CountAsync(g => g.ArqueoId == null);

        public async Task GuardarCierreCajaAsync(Arqueo nuevoArqueo)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    nuevoArqueo.SaldoFinalDia = nuevoArqueo.SaldoInicial + nuevoArqueo.TotEntradas - nuevoArqueo.TotSalidas;
                    nuevoArqueo.Descuadre = nuevoArqueo.TotalConteoDinero - (nuevoArqueo.SaldoInicial + nuevoArqueo.TotEntradas - nuevoArqueo.TotSalidas);
                    _context.Add(nuevoArqueo);
                    await _context.SaveChangesAsync();

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

        // --- HISTORIAL ---
        public async Task<List<Arqueo>> ObtenerHistorialCierresAsync()
        {
            return await _context.Arqueos.Include(a => a.Usuario)
                .OrderByDescending(a => a.FechaArqueo).ThenByDescending(a => a.FechaHora).Take(50).ToListAsync();
        }

        public async Task<Arqueo?> ObtenerDetalleCierreAsync(int arqueoId)
        {
            return await _context.Arqueos.Include(a => a.Usuario).FirstOrDefaultAsync(a => a.Id == arqueoId);
        }

        // --- REPORTES EXCEL ---

        public async Task<byte[]> GenerarReporteExcelCierreAsync(int arqueoId)
        {
            var arqueo = await _context.Arqueos.FindAsync(arqueoId);
            if (arqueo == null) return Array.Empty<byte>();

            var entradas = await _context.Entradas.Where(e => e.ArqueoId == arqueoId).ToListAsync();
            var gastos = await _context.Gastos.Where(g => g.ArqueoId == arqueoId).ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add($"Cierre {arqueo.Id}");

            var moneyFormat = "$ #,##0.00";

            ws.Cell("A1").Value = "REPORTE DE CIERRE DE CAJA";
            ws.Range("A1:E1").Merge();

            ws.Cell("A3").Value = "ID Cierre:"; ws.Cell("B3").Value = arqueo.Id;
            ws.Cell("A4").Value = "Fecha:"; ws.Cell("B4").Value = arqueo.FechaArqueo.ToShortDateString();
            ws.Cell("A5").Value = "Responsable:"; ws.Cell("B5").Value = arqueo.UsuarioId;
            ws.Cell("A6").Value = "Generado:"; ws.Cell("B6").Value = DateTime.Now.ToString();

            int row = 8;
            ws.Cell(row, 1).Value = "RESUMEN FINANCIERO";
            row++;

            ws.Cell(row, 1).Value = "Saldo Inicial:";
            ws.Cell(row, 2).Value = arqueo.SaldoInicial; ws.Cell(row, 2).Style.NumberFormat.Format = moneyFormat; row++;

            ws.Cell(row, 1).Value = "(+) Entradas:";
            ws.Cell(row, 2).Value = arqueo.TotEntradas; ws.Cell(row, 2).Style.NumberFormat.Format = moneyFormat; row++;

            ws.Cell(row, 1).Value = "(-) Salidas:";
            ws.Cell(row, 2).Value = arqueo.TotSalidas; ws.Cell(row, 2).Style.NumberFormat.Format = moneyFormat; row++;

            ws.Cell(row, 1).Value = "(=) Saldo Sistema:";
            ws.Cell(row, 2).Value = (arqueo.SaldoInicial + arqueo.TotEntradas - arqueo.TotSalidas);
            ws.Cell(row, 2).Style.NumberFormat.Format = moneyFormat; row++;

            ws.Cell(row, 1).Value = "Conteo Físico:";
            ws.Cell(row, 2).Value = arqueo.TotalConteoDinero;
            ws.Cell(row, 2).Style.NumberFormat.Format = moneyFormat; row++;

            ws.Cell(row, 1).Value = "DIFERENCIA:";
            ws.Cell(row, 2).Value = arqueo.Descuadre;
            ws.Cell(row, 2).Style.NumberFormat.Format = moneyFormat;

            // Detalle de Entradas
            row += 3;
            ws.Cell(row, 1).Value = "DETALLE DE ENTRADAS";
            row++;

            ws.Cell(row, 1).Value = "ID"; ws.Cell(row, 2).Value = "Hora"; ws.Cell(row, 3).Value = "Concepto"; ws.Cell(row, 4).Value = "Monto";
            row++;

            foreach (var e in entradas)
            {
                ws.Cell(row, 1).Value = e.Id;
                ws.Cell(row, 2).Value = e.Fecha.ToString("HH:mm");
                ws.Cell(row, 3).Value = e.Concepto;
                ws.Cell(row, 4).Value = e.Monto; ws.Cell(row, 4).Style.NumberFormat.Format = moneyFormat;
                row++;
            }

            // Detalle de Salidas
            row += 2;
            ws.Cell(row, 1).Value = "DETALLE DE SALIDAS";
            row++;

            ws.Cell(row, 1).Value = "Ref"; ws.Cell(row, 2).Value = "Hora"; ws.Cell(row, 3).Value = "Beneficiario"; ws.Cell(row, 4).Value = "Concepto"; ws.Cell(row, 5).Value = "Monto";
            row++;

            foreach (var g in gastos)
            {
                ws.Cell(row, 1).Value = $"{g.Prefijo}-{g.Consecutivo}";
                ws.Cell(row, 2).Value = g.Fecha.ToString("HH:mm");
                ws.Cell(row, 3).Value = g.Beneficiario;
                ws.Cell(row, 4).Value = g.Concepto;
                ws.Cell(row, 5).Value = g.Monto; ws.Cell(row, 5).Style.NumberFormat.Format = moneyFormat;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream.ToArray();
        }

        public async Task<List<TransaccionDTO>> ObtenerMovimientosPorRangoAsync(DateTime inicio, DateTime fin)
        {
            var fechaFin = fin.Date.AddDays(1).AddTicks(-1);
            var fechaInicio = inicio.Date;

            var entradas = await _context.Entradas.Include(e => e.Usuario).Where(e => e.Fecha >= fechaInicio && e.Fecha <= fechaFin).ToListAsync();
            var gastos = await _context.Gastos.Include(g => g.Usuario).Where(g => g.Fecha >= fechaInicio && g.Fecha <= fechaFin).ToListAsync();

            var movimientos = new List<TransaccionDTO>();

            foreach (var e in entradas)
            {
                movimientos.Add(new TransaccionDTO
                {
                    Id = e.Id,
                    Fecha = e.Fecha,
                    Tipo = "Entrada",
                    Referencia = $"ENT-{e.Id}",
                    Concepto = e.Concepto,
                    Detalle = "Ingreso",
                    Monto = e.Monto,
                    Usuario = e.UsuarioId,
                    Anulado = e.Anulado
                });
            }

            foreach (var g in gastos)
            {
                movimientos.Add(new TransaccionDTO
                {
                    Id = g.Id,
                    Fecha = g.Fecha,
                    Tipo = "Salida",
                    Referencia = $"{g.Prefijo}-{g.Consecutivo}",
                    Concepto = g.Concepto,
                    Detalle = g.Beneficiario,
                    Monto = g.Monto,
                    Usuario = g.UsuarioId,
                    Anulado = g.Anulado
                });
            }
            return movimientos.OrderByDescending(m => m.Fecha).ToList();
        }

        public async Task<byte[]> GenerarReporteExcelMovimientosAsync(DateTime inicio, DateTime fin)
        {
            var movimientos = await ObtenerMovimientosPorRangoAsync(inicio, fin);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Movimientos");

            var moneyFormat = "$ #,##0.00";

            ws.Cell("A1").Value = "REPORTE DE MOVIMIENTOS";
            ws.Range("A1:G1").Merge();

            ws.Cell("A3").Value = "Desde:"; ws.Cell("B3").Value = inicio.ToShortDateString();
            ws.Cell("C3").Value = "Hasta:"; ws.Cell("D3").Value = fin.ToShortDateString();
            ws.Cell("F3").Value = "Generado:"; ws.Cell("G3").Value = DateTime.Now.ToString();

            int row = 5;
            ws.Cell(row, 1).Value = "Fecha y Hora";
            ws.Cell(row, 2).Value = "Tipo";
            ws.Cell(row, 3).Value = "Referencia";
            ws.Cell(row, 4).Value = "Concepto";
            ws.Cell(row, 5).Value = "Beneficiario / Detalle";
            ws.Cell(row, 6).Value = "Usuario";
            ws.Cell(row, 7).Value = "Monto";
            ws.Cell(row, 8).Value = "Estado";

            row++;

            foreach (var m in movimientos)
            {
                ws.Cell(row, 1).Value = m.Fecha;
                ws.Cell(row, 2).Value = m.Tipo;
                ws.Cell(row, 3).Value = m.Referencia;
                ws.Cell(row, 4).Value = m.Concepto;
                ws.Cell(row, 5).Value = m.Detalle;
                ws.Cell(row, 6).Value = m.Usuario;
                ws.Cell(row, 7).Value = m.Monto;
                ws.Cell(row, 7).Style.NumberFormat.Format = moneyFormat;

                if (m.Tipo == "Entrada") ws.Cell(row, 7).Style.Font.SetFontColor(XLColor.Green);
                else ws.Cell(row, 7).Style.Font.SetFontColor(XLColor.Red);

                ws.Cell(row, 8).Value = m.Estado;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream.ToArray();
        }

        // --- KPIs CONTADOR ---
        public async Task<ContadorKPI> ObtenerKPIsContadorAsync()
        {
            var mesActual = DateTime.Today.Month;
            var yearActual = DateTime.Today.Year;
            var primerDiaMes = new DateTime(yearActual, mesActual, 1);

            var ingresos = await _context.Entradas
                .Where(e => e.Fecha >= primerDiaMes && !e.Anulado)
                .SumAsync(e => e.Monto);

            var gastos = await _context.Gastos
                .Where(g => g.Fecha >= primerDiaMes && !g.Anulado)
                .SumAsync(g => g.Monto);

            var arqueoSimulado = await SimularCierreActualAsync();
            var saldoCaja = arqueoSimulado.SaldoInicial + arqueoSimulado.TotEntradas - arqueoSimulado.TotSalidas;

            var pendientes = await _context.Entradas.CountAsync(e => e.ArqueoId == null) +
                             await _context.Gastos.CountAsync(g => g.ArqueoId == null);

            return new ContadorKPI
            {
                IngresosMes = ingresos,
                GastosMes = gastos,
                SaldoCaja = saldoCaja,
                PendientesCierre = pendientes
            };
        }

        // Auditoría
        public async Task<List<TransaccionDTO>> ObtenerMovimientosPendientesAsync()
        {
            var entradas = await _context.Entradas.Include(e => e.Usuario).Where(e => e.ArqueoId == null).ToListAsync();
            var gastos = await _context.Gastos.Include(g => g.Usuario).Where(g => g.ArqueoId == null).ToListAsync();

            var list = new List<TransaccionDTO>();
            foreach (var e in entradas) list.Add(new TransaccionDTO { Id = e.Id, Fecha = e.Fecha, Tipo = "Entrada", Referencia = $"ENT-{e.Id}", Concepto = e.Concepto, Detalle = "Ingreso", Monto = e.Monto, Usuario = e.UsuarioId, Anulado = e.Anulado });
            foreach (var g in gastos) list.Add(new TransaccionDTO { Id = g.Id, Fecha = g.Fecha, Tipo = "Salida", Referencia = $"{g.Prefijo}-{g.Consecutivo}", Concepto = g.Concepto, Detalle = g.Beneficiario, Monto = g.Monto, Usuario = g.UsuarioId, Anulado = g.Anulado });

            return list.OrderByDescending(x => x.Fecha).ToList();
        }

        public async Task AnularTransaccionAsync(int id, string tipo)
        {
            if (tipo == "Entrada")
            {
                var ent = await _context.Entradas.FindAsync(id);
                if (ent != null && ent.ArqueoId == null)
                {
                    ent.Anulado = true;
                    await _context.SaveChangesAsync();
                }
                else throw new Exception("No se puede anular: Transacción no encontrada o ya cerrada.");
            }
            else
            {
                var gasto = await _context.Gastos.FindAsync(id);
                if (gasto != null && gasto.ArqueoId == null)
                {
                    gasto.Anulado = true;
                    await _context.SaveChangesAsync();
                }
                else throw new Exception("No se puede anular: Transacción no encontrada o ya cerrada.");
            }
        }

        public async Task ModificarMontoTransaccionAsync(int id, string tipo, decimal nuevoMonto)
        {
            if (tipo == "Entrada")
            {
                var ent = await _context.Entradas.FindAsync(id);
                if (ent != null && ent.ArqueoId == null)
                {
                    ent.Monto = nuevoMonto;
                    await _context.SaveChangesAsync();
                }
                else throw new Exception("No se puede editar: Transacción cerrada.");
            }
            else
            {
                var gasto = await _context.Gastos.FindAsync(id);
                if (gasto != null && gasto.ArqueoId == null)
                {
                    gasto.Monto = nuevoMonto;
                    await _context.SaveChangesAsync();
                }
                else throw new Exception("No se puede editar: Transacción cerrada.");
            }
        }
    }
}