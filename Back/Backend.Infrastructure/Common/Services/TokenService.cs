using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Backend.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Infrastructure.Common.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateJwtToken(int idUsuario, string correo, string rolGlobal)
    {
        var key      = _config["Jwt:Key"]      ?? throw new InvalidOperationException("Jwt:Key no configurado");
        var issuer   = _config["Jwt:Issuer"]   ?? throw new InvalidOperationException("Jwt:Issuer no configurado");
        var audience = _config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience no configurado");
        var expiry   = int.Parse(_config["Jwt:ExpiryInHours"] ?? "8");

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, idUsuario.ToString()),
            new(JwtRegisteredClaimNames.Email, correo),
            new("rolGlobal", rolGlobal),
            new(ClaimTypes.Role, rolGlobal), // requerido para [Authorize(Roles = "Admin")]
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiry),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public string GenerateSixDigitCode()
    {
        var bytes = new byte[4];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var num = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
        return num.ToString("D6");
    }
}
