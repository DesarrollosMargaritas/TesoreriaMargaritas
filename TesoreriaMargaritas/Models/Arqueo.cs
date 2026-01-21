using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Collections.Generic;

namespace TesoreriaMargaritas.Models
{
    [Table("Arqueos")]
    public class Arqueo
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaCierre { get; set; } = DateTime.Now;

        // Propiedad adicional para compatibilidad con código nuevo
        public DateTime FechaHora { get; set; } = DateTime.Now;
        public DateTime FechaArqueo { get; set; } = DateTime.Today;

        // Quién hizo el cierre
        [Required]
        [MaxLength(20)]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        public int CajaId { get; set; } = 1;

        // El dinero base fijo de la caja (ej. 1.000.000)
        [Column(TypeName = "decimal(18, 2)")]
        public decimal BaseCaja { get; set; }


        // --- EFECTIVO (Lógica de Arrastre) ---
        // NUEVO: Lo que sobró o faltó del arqueo PASADO.
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SaldoArrastreAnterior { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SaldoInicialEfectivo { get; set; } // Físico ayer

        // --- SALDOS INICIALES DIGITALES ---
        // Estas propiedades son cruciales para la continuidad de saldos en Nequi y Daviplata
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SaldoInicialNequi { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SaldoInicialDaviplata { get; set; }


        // --- TOTALES SISTEMA POR MEDIO DE PAGO ---

        // Efectivo
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SistEntradasEfectivo { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SistSalidasEfectivo { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SistTotalEfectivo { get; set; } // (Inicial + Ent - Sal)

        // Nequi
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SistEntradasNequi { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SistSalidasNequi { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SistTotalNequi { get; set; } // (Ent - Sal)

        // Daviplata
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SistEntradasDaviplata { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SistSalidasDaviplata { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SistTotalDaviplata { get; set; } // (Ent - Sal)


        // --- CONTEO FÍSICO / REPORTADO ---

        [Required]
        public string ConteoBilletesJson { get; set; } = "{}"; // Detalle billetes efectivo

        [Column(TypeName = "decimal(18, 2)")]
        public decimal FisicoEfectivo { get; set; } // Total sumado billetes

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ReportadoNequi { get; set; } // Lo que el usuario dice que hay en App

        [Column(TypeName = "decimal(18, 2)")]
        public decimal ReportadoDaviplata { get; set; }


        // --- RESULTADOS DEL CRUCE (Descuadres) ---
        [Column(TypeName = "decimal(18, 2)")]
        public decimal DescuadreEfectivo { get; set; } // Fisico - Sistema

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DescuadreNequi { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DescuadreDaviplata { get; set; }

        // Inicializamos para evitar el error "Cannot insert NULL"
        public string? Observaciones { get; set; } = string.Empty;

        // --- PROPIEDADES LEGACY / ALIAS ---
        // Estas propiedades existen para que el código antiguo siga compilando,
        // pero internamente leen/escriben a las nuevas columnas.

        [NotMapped]
        public decimal SaldoInicial
        {
            get => SaldoInicialEfectivo;
            set => SaldoInicialEfectivo = value;
        }

        [NotMapped]
        public decimal TotalEntradas
        {
            get => SistEntradasEfectivo + SistEntradasNequi + SistEntradasDaviplata;
            set { /* Opcional: podrías decidir cómo distribuir, pero mejor dejar vacío */ }
        }

        // Alias para compatibilidad con código que usa "TotEntradas"
        [NotMapped]
        public decimal TotEntradas
        {
            get => TotalEntradas;
            set => TotalEntradas = value;
        }

        [NotMapped]
        public decimal TotalGastos
        {
            get => SistSalidasEfectivo + SistSalidasNequi + SistSalidasDaviplata;
            set { }
        }

        // Alias para compatibilidad con código que usa "TotSalidas"
        [NotMapped]
        public decimal TotSalidas
        {
            get => TotalGastos;
            set => TotalGastos = value;
        }

        [NotMapped]
        public decimal SaldoSistema
        {
            get => SistTotalEfectivo + SistTotalNequi + SistTotalDaviplata;
            set { }
        }

        // Alias
        [NotMapped]
        public decimal SaldoFinalDia
        {
            get => SaldoSistema;
            set => SaldoSistema = value;
        }

        [NotMapped]
        public decimal SaldoFisico
        {
            get => FisicoEfectivo; // Asumimos que el saldo físico principal es efectivo
            set => FisicoEfectivo = value;
        }

        // Alias
        [NotMapped]
        public decimal TotalConteoDinero
        {
            get => SaldoFisico;
            set => SaldoFisico = value;
        }

        [NotMapped]
        public decimal Diferencia
        {
            get => DescuadreEfectivo + DescuadreNequi + DescuadreDaviplata;
            set { }
        }

        // Alias
        [NotMapped]
        public decimal Descuadre
        {
            get => Diferencia;
            set => Diferencia = value;
        }

        [NotMapped] public decimal TotEntradasAnu { get; set; }
        [NotMapped] public decimal TotSalidasAnu { get; set; }

        // Propiedad Auxiliar JSON
        [NotMapped]
        public Dictionary<string, int> DetalleConteo
        {
            get => string.IsNullOrEmpty(ConteoBilletesJson)
                   ? new Dictionary<string, int>()
                   : JsonSerializer.Deserialize<Dictionary<string, int>>(ConteoBilletesJson) ?? new Dictionary<string, int>();
            set => ConteoBilletesJson = JsonSerializer.Serialize(value);
        }
    }
}