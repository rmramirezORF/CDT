using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Proyectos.Commands;

public class ActualizarProyectoCommand
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "La descripción no puede superar 2000 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El equipo es obligatorio.")]
    public int IdEquipo { get; set; }
}
