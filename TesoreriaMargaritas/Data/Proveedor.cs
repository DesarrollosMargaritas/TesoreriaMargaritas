using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace TesoreriaMargaritas.Models;

[Table("PROVEEDORES")]
public class Proveedor
{
    [Key]
    public string CODPROVEEDOR { get; set; } = string.Empty;

    public string NOMPROVEEDOR { get; set; } = string.Empty;
}
