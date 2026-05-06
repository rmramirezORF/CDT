using Backend.Application.Auth.Commands;
using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Auth.UseCases;

public class ConfirmEmailUseCase
{
    private readonly ICdtDbContext _context;

    public ConfirmEmailUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(ConfirmEmailCommand cmd, CancellationToken ct = default)
    {
        var correo = cmd.Correo.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo, ct);
        if (usuario is null)
            throw new BusinessException("USER_NOT_FOUND", "No existe un usuario con ese correo.");

        if (usuario.FechaConfirmacionEmail is not null)
            return; // idempotente: ya estaba confirmado

        if (string.IsNullOrEmpty(usuario.CodigoConfirmacionEmail) || usuario.CodigoConfirmacionEmail != cmd.Codigo)
            throw new BusinessException("EMAIL_CONFIRMATION_FAILED", "El código no es válido.");

        if (usuario.FechaExpiracionConfirmacion is null || usuario.FechaExpiracionConfirmacion < DateTime.UtcNow)
            throw new BusinessException("EMAIL_CONFIRMATION_FAILED", "El código ha expirado. Solicita uno nuevo.");

        usuario.FechaConfirmacionEmail = DateTime.UtcNow;
        usuario.CodigoConfirmacionEmail = null;
        usuario.FechaExpiracionConfirmacion = null;

        await _context.SaveChangesAsync(ct);
    }
}
