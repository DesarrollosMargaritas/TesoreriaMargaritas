using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesoreriaMargaritas.Models
{
    [Table("Entradas")]
    public class Entrada
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio")]
        [MaxLength(200)]
        public string Concepto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(50, double.MaxValue, ErrorMessage = "El monto mínimo es $50")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        // --- NUEVO CAMPO ---
        [Required(ErrorMessage = "Seleccione forma de pago")]
        [MaxLength(20)]
        public string FormaPago { get; set; } = "Efectivo"; // Efectivo, Nequi, Daviplata

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }
        [ForeignKey("ArqueoId")]
        public int? ArqueoId { get; set; }

        public virtual Arqueo Arqueo { get; set; }

        public bool Anulado { get; set; } = false;

        [MaxLength(500)]
        public string? Observaciones { get; set; }
    }
}