using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesoreriaMargaritas.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        [MaxLength(20)]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        // Valores esperados: "Tesorero", "Contador", "Administrador"
        public string Rol { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;

        // Relaciones de Auditoría (opcional, pero útil para navegar)
        public virtual ICollection<Entrada>? EntradasRegistradas { get; set; }
        // public virtual ICollection<Gasto>? GastosRegistrados { get; set; }
        // public virtual ICollection<Arqueo>? ArqueosRealizados { get; set; }
    }
}