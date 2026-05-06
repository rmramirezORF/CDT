namespace Backend.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(int idUsuario, string correo, string rolGlobal);
    string GenerateRefreshToken();
    string GenerateSixDigitCode();
}
