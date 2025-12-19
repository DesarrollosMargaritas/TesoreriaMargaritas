using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesoreriaMargaritas.Models
{
    [Table("Cajas")]
    public class Caja
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = "Funza Principal";

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SaldoActual { get; set; }

        [MaxLength(20)]
        public string Estado { get; set; } = "Abierta"; // Abierta, Cerrada
    }
}