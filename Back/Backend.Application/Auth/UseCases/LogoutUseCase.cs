using Backend.Application.Auth.Commands;
using Backend.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Auth.UseCases;

public class LogoutUseCase
{
    private readonly ICdtDbContext _context;

    public LogoutUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(LogoutCommand cmd, CancellationToken ct = default)
    {
        var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == cmd.RefreshToken, ct);
        if (token is null || token.Revocado) return; // idempotente

        token.Revocado = true;
        await _context.SaveChangesAsync(ct);
    }
}
