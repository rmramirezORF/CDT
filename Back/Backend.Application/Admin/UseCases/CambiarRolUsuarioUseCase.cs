using Backend.Application.Admin.Commands;
using Backend.Application.Admin.DTOs;
using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Admin.UseCases;

public class CambiarRolUsuarioUseCase
{
    private readonly ICdtDbContext _context;

    public CambiarRolUsuarioUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<UsuarioListItemDto> ExecuteAsync(int idUsuario, CambiarRolUsuarioCommand cmd, CancellationToken ct = default)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == idUsuario, ct);
        if (usuario is null)
            throw new BusinessException("USER_NOT_FOUND", "El usuario no existe.");

        usuario.RolGlobal = cmd.RolGlobal;
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
