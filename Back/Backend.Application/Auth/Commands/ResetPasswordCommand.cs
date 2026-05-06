using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Auth.Commands;

public class ResetPasswordCommand
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener 6 dígitos.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
    public string NuevaPassword { get; set; } = string.Empty;
}
