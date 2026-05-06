using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Listas.Commands;

public class CrearListaCommand
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El color no puede superar 20 caracteres.")]
    public string? Color { get; set; }

    [Required(ErrorMessage = "El proyecto es obligatorio.")]
    public int IdProyecto { get; set; }

    /// <summary>
    /// Orden opcional. Si no se envía, se asigna al final (max orden + 1).
    /// </summary>
    public int? Orden { get; set; }
}
