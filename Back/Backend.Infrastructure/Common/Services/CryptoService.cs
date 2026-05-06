using Backend.Application.Common.Interfaces;

namespace Backend.Infrastructure.Common.Services;

public class CryptoService : ICryptoService
{
    public string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    public bool VerifyPassword(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}
