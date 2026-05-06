namespace Backend.Application.Auth.DTOs;

public class RefreshTokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
