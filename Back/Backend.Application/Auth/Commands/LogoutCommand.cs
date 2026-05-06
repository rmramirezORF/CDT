using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Auth.Commands;

public class LogoutCommand
{
    [Required(ErrorMessage = "El refresh token es obligatorio.")]
    public string RefreshToken { get; set; } = string.Empty;
}
