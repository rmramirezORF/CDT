using Backend.Application.Auth.Commands;
using Backend.Application.Auth.DTOs;
using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Persistence;
using Backend.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Auth.UseCases;

public class RefreshTokenUseCase
{
    private const int RefreshExpiryDays = 7;

    private readonly ICdtDbContext _context;
    private readonly ITokenService _tokens;

    public RefreshTokenUseCase(ICdtDbContext context, ITokenService tokens)
    {
        _context = context;
        _tokens = tokens;
    }

    public async Task<RefreshTokenResponseDto> ExecuteAsync(RefreshTokenCommand cmd, CancellationToken ct = default)
    {
        var token = await _context.RefreshTokens
            .Include(rt => rt.Usuario)
            .FirstOrDefaultAsync(rt => rt.Token == cmd.RefreshToken, ct);

        if (token is null || token.Revocado || token.FechaExpiracion < DateTime.UtcNow)
            throw new BusinessException("REFRESH_TOKEN_FAILED", "Token inválido o expirado.");

        if (!token.Usuario.Estado)
            throw new BusinessException("USER_INACTIVE", "La cuenta está inactiva.");

        // Rotar: revocar el anterior y emitir uno nuevo.
        token.Revocado = true;

        var nuevoJwt = _tokens.GenerateJwtToken(token.Usuario.Id, token.Usuario.Correo, token.Usuario.RolGlobal);
        var nuevoRefresh = _tokens.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshTokenEntity
        {
            IdUsuario = token.IdUsuario,
            Token = nuevoRefresh,
            FechaExpiracion = DateTime.UtcNow.AddDays(RefreshExpiryDays),
        });
        await _context.SaveChangesAsync(ct);

        return new RefreshTokenResponseDto
        {
            Token = nuevoJwt,
            RefreshToken = nuevoRefresh,
        };
    }
}
