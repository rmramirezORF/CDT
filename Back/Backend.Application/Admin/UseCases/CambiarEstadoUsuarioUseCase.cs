using Backend.Application.Admin.Commands;
using Backend.Application.Admin.DTOs;
using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Admin.UseCases;

public class CambiarEstadoUsuarioUseCase
{
    private readonly ICdtDbContext _context;

    public CambiarEstadoUsuarioUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<UsuarioListItemDto> ExecuteAsync(int idUsuario, CambiarEstadoUsuarioCommand cmd, CancellationToken ct = default)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == idUsuario, ct);
        if (usuario is null)
            throw new BusinessException("USER_NOT_FOUND", "El usuario no existe.");

        usuario.Estado = cmd.Estado;

        if (!cmd.Estado)
        {
            // Revocar refresh tokens activos al desactivar la cuenta
            await _context.RefreshTokens
                .Where(rt => rt.IdUsuario == usuario.Id && !rt.Revocado)
                .ForEachAsync(rt => rt.Revocado = true, ct);
        }

        await _context.SaveChangesAsync(ct);

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
