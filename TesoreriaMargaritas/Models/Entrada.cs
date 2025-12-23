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

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        // --- CORRECCIÓN: Agregamos el campo que causaba el error ---
        public int? ArqueoId { get; set; }
    }
}