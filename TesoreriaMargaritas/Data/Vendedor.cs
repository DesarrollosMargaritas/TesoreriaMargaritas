using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace TesoreriaMargaritas.Models;

[Table("VENDEDORES")]
public class Vendedor
{
    [Key]
    public string CODVENDEDOR { get; set; } = string.Empty;

    public string NOMVENDEDOR { get; set; } = string.Empty;
}