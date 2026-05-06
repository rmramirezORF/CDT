using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Auth.Commands;

public class ConfirmEmailCommand
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener 6 dígitos.")]
    public string Codigo { get; set; } = string.Empty;
}
