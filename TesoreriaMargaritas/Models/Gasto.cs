using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesoreriaMargaritas.Models
{
    [Table("Gastos")]
    public class Gasto
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Seleccione un concepto")]
        [MaxLength(50)]
        public string Concepto { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El beneficiario es obligatorio")]
        [MaxLength(200)]
        public string Beneficiario { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Prefijo { get; set; } = string.Empty;

        [Required]
        public int Consecutivo { get; set; }

        public bool Anulado { get; set; } = false;

        [MaxLength(500)]
        public string Observaciones { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        // --- CORRECCIÓN: Agregamos el campo que causaba el error ---
        public int? ArqueoId { get; set; }
    }
}