using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Admin.Commands;

public class ActualizarCatalogoItemCommand
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El color es obligatorio.")]
    [StringLength(20, MinimumLength = 4, ErrorMessage = "El color debe tener entre 4 y 20 caracteres.")]
    public string Color { get; set; } = "#6b7280";

    public int Orden { get; set; }
}
