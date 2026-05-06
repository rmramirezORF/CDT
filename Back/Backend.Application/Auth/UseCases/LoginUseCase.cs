using Backend.Application.Auth.Commands;
using Backend.Application.Auth.DTOs;
using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Persistence;
using Backend.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Auth.UseCases;

public class LoginUseCase
{
    private const int RefreshExpiryDays = 7;

    private readonly ICdtDbContext _context;
    private readonly ICryptoService _crypto;
    private readonly ITokenService _tokens;

    public LoginUseCase(ICdtDbContext context, ICryptoService crypto, ITokenService tokens)
    {
        _context = context;
        _crypto = crypto;
        _tokens = tokens;
    }

    public async Task<LoginResponseDto> ExecuteAsync(LoginCommand cmd, CancellationToken ct = default)
    {
        var correo = cmd.Correo.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo, ct);

        if (usuario is null || !_crypto.VerifyPassword(cmd.Password, usuario.ClaveHash))
            throw new BusinessException("LOGIN_FAILED", "Correo o contraseña incorrectos.");

        if (!usuario.Estado)
            throw new BusinessException("USER_INACTIVE", "Tu cuenta está inactiva. Contacta al administrador.");

        if (usuario.FechaConfirmacionEmail is null)
            throw new BusinessException("EMAIL_NOT_CONFIRMED", "Debes confirmar tu correo antes de iniciar sesión.");

        var jwt = _tokens.GenerateJwtToken(usuario.Id, usuario.Correo, usuario.RolGlobal);
        var refresh = _tokens.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshTokenEntity
        {
            IdUsuario = usuario.Id,
            Token = refresh,
            FechaExpiracion = DateTime.UtcNow.AddDays(RefreshExpiryDays),
        });
        await _context.SaveChangesAsync(ct);

        return new LoginResponseDto
        {
            Token = jwt,
            RefreshToken = refresh,
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                RolGlobal = usuario.RolGlobal,
                Estado = usuario.Estado,
                FechaCreacion = usuario.FechaCreacion,
                FechaConfirmacionEmail = usuario.FechaConfirmacionEmail,
            },
        };
    }
}
