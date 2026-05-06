using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Admin.Commands;

public class CambiarRolUsuarioCommand
{
    /// <summary>Rol global: Admin, Lider o Miembro.</summary>
    [Required(ErrorMessage = "El rol es obligatorio.")]
    [RegularExpression("^(Admin|Lider|Miembro)$", ErrorMessage = "Rol inválido. Debe ser Admin, Lider o Miembro.")]
    public string RolGlobal { get; set; } = string.Empty;
}
