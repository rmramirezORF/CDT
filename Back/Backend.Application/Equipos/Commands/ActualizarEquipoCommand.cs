using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Equipos.Commands;

public class ActualizarEquipoCommand
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    public int? IdEquipoPadre { get; set; }
    public int? IdLider { get; set; }
}
