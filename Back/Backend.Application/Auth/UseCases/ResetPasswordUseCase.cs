using Backend.Application.Auth.Commands;
using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Interfaces;
using Backend.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Auth.UseCases;

public class ResetPasswordUseCase
{
    private readonly ICdtDbContext _context;
    private readonly ICryptoService _crypto;

    public ResetPasswordUseCase(ICdtDbContext context, ICryptoService crypto)
    {
        _context = context;
        _crypto = crypto;
    }

    public async Task ExecuteAsync(ResetPasswordCommand cmd, CancellationToken ct = default)
    {
        var correo = cmd.Correo.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo, ct);
        if (usuario is null)
            throw new BusinessException("RESET_TOKEN_INVALID", "Código o correo inválido.");

        var token = await _context.TokensResetPassword
            .Where(t => t.IdUsuario == usuario.Id
                     && t.Codigo6Digitos == cmd.Codigo
                     && !t.Usado)
            .OrderByDescending(t => t.FechaCreacion)
            .FirstOrDefaultAsync(ct);

        if (token is null)
            throw new BusinessException("RESET_TOKEN_INVALID", "Código o correo inválido.");

        if (token.FechaExpiracion < DateTime.UtcNow)
            throw new BusinessException("RESET_TOKEN_EXPIRED", "El código ha expirado. Solicita uno nuevo.");

        usuario.ClaveHash = _crypto.HashPassword(cmd.NuevaPassword);
        token.Usado = true;

        await _context.SaveChangesAsync(ct);
    }
}
