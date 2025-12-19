using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json; // Necesario para manejar el JSON

namespace TesoreriaMargaritas.Models
{
    [Table("Arqueos")]
    public class Arqueo
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaHora { get; set; } = DateTime.Now;

        [Required]
        public DateTime FechaArqueo { get; set; } // La fecha contable que se está cerrando

        // Totales Calculados
        [Column(TypeName = "decimal(18, 2)")]
        public decimal SaldoInicial { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotEntradas { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotEntradasAnu { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotSalidas { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotSalidasAnu { get; set; }

        // Conteo Físico (JSON en Base de Datos)
        [Required]
        public string ConteoDinero { get; set; } = "{}";

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalConteoDinero { get; set; }

        // Resultados
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Descuadre { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SaldoFinalDia { get; set; }

        // Relaciones
        [Required]
        [MaxLength(20)]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public virtual Usuario? Usuario { get; set; }

        public int CajaId { get; set; } = 1;

        [ForeignKey("CajaId")]
        public virtual Caja? Caja { get; set; }

        // PROPIEDAD AUXILIAR (No va a BD)
        // Nos permite trabajar con el diccionario de billetes en C# cómodamente
        [NotMapped]
        public Dictionary<string, int> DetalleConteo
        {
            get => string.IsNullOrEmpty(ConteoDinero)
                   ? new Dictionary<string, int>()
                   : JsonSerializer.Deserialize<Dictionary<string, int>>(ConteoDinero) ?? new Dictionary<string, int>();
            set => ConteoDinero = JsonSerializer.Serialize(value);
        }
    }
}