using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Tareas.Commands;

public class ActualizarTareaCommand
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(300, MinimumLength = 2, ErrorMessage = "El título debe tener entre 2 y 300 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(4000, ErrorMessage = "La descripción no puede superar 4000 caracteres.")]
    public string? Descripcion { get; set; }

    /// <summary>Mover tarea a otra lista (drag & drop entre columnas Kanban). Debe pertenecer al mismo proyecto.</summary>
    public int? IdLista { get; set; }

    public int? IdTipoActividad { get; set; }
    public int? IdEstado { get; set; }
    public int? IdPrioridad { get; set; }
    public int? IdResponsable { get; set; }
    public DateTime? FechaVencimiento { get; set; }
}
