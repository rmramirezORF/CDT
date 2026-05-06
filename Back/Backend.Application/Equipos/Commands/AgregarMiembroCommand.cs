using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Equipos.Commands;

public class AgregarMiembroCommand
{
    [Required(ErrorMessage = "El id de usuario es obligatorio.")]
    public int IdUsuario { get; set; }
}
