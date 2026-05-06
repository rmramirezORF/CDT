namespace Backend.Application.Auth.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UsuarioDto Usuario { get; set; } = null!;
}
