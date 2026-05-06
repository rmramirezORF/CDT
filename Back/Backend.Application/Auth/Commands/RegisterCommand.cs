using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Auth.Commands;

public class RegisterCommand
{
    /// <summary>Nombre completo del usuario.</summary>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Correo electrónico (será su usuario de login).</summary>
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(120, ErrorMessage = "El correo debe tener máximo 120 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    /// <summary>Contraseña en texto plano (se hashea con BCrypt).</summary>
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
    public string Password { get; set; } = string.Empty;
}
