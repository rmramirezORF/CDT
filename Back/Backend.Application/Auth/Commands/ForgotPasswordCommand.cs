using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Auth.Commands;

public class ForgotPasswordCommand
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    public string Correo { get; set; } = string.Empty;
}
