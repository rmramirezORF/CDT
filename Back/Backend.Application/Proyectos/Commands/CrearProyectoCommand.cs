using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Proyectos.Commands;

public class CrearProyectoCommand
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La clave es obligatoria.")]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "La clave debe tener entre 2 y 10 caracteres.")]
    [RegularExpression("^[A-Z0-9]+$", ErrorMessage = "La clave debe contener solo letras mayúsculas y números.")]
    public string Clave { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "La descripción no puede superar 2000 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El equipo es obligatorio.")]
    public int IdEquipo { get; set; }
}
