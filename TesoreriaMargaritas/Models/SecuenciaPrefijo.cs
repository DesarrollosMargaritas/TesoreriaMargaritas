using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesoreriaMargaritas.Models
{
    // Tabla auxiliar para controlar el número consecutivo de cada prefijo (GAP, SMP, NMP)
    [Table("SecuenciasPrefijos")]
    public class SecuenciaPrefijo
    {
        [Key]
        [MaxLength(10)]
        public string Prefijo { get; set; } = string.Empty; // GAP, SMP, NMP

        public int UltimoConsecutivo { get; set; }
    }
}