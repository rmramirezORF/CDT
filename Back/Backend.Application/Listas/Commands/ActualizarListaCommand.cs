using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Listas.Commands;

public class ActualizarListaCommand
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El color no puede superar 20 caracteres.")]
    public string? Color { get; set; }
}
