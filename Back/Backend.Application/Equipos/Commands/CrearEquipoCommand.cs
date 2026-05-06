using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Equipos.Commands;

public class CrearEquipoCommand
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Id del equipo padre. Null para equipos raíz.</summary>
    public int? IdEquipoPadre { get; set; }

    /// <summary>Id del usuario líder. Null si aún no se asigna.</summary>
    public int? IdLider { get; set; }
}
