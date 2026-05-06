using Backend.Application.Admin.DTOs;
using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Admin.UseCases;

/// <summary>
/// Permite a un Admin confirmar el email de un usuario sin necesidad de que reciba/use
/// el código por correo. Útil cuando hay problemas con el SMTP o la persona no tiene
/// acceso a su bandeja todavía.
/// </summary>
public class ConfirmarEmailManualmenteUseCase
{
    private readonly ICdtDbContext _context;

    public ConfirmarEmailManualmenteUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<UsuarioListItemDto> ExecuteAsync(int idUsuario, CancellationToken ct = default)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == idUsuario, ct);
        if (usuario is null)
            throw new BusinessException("USER_NOT_FOUND", "El usuario no existe.");

        // Idempotente: si ya estaba confirmado, devolvemos su DTO sin tocar nada.
        if (usuario.FechaConfirmacionEmail is null)
        {
            usuario.FechaConfirmacionEmail = DateTime.UtcNow;
            usuario.CodigoConfirmacionEmail = null;
            usuario.FechaExpiracionConfirmacion = null;
            await _context.SaveChangesAsync(ct);
        }

        return new UsuarioListItemDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            RolGlobal = usuario.RolGlobal,
            Estado = usuario.Estado,
            FechaCreacion = usuario.FechaCreacion,
            FechaConfirmacionEmail = usuario.FechaConfirmacionEmail,
        };
    }
}
