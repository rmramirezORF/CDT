using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Admin.UseCases;

public class EliminarUsuarioUseCase
{
    private readonly ICdtDbContext _context;

    public EliminarUsuarioUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(int idUsuarioEliminar, int idUsuarioActual, CancellationToken ct = default)
    {
        if (idUsuarioEliminar == idUsuarioActual)
            throw new BusinessException("CANNOT_DELETE_SELF", "No puedes eliminar tu propia cuenta.");

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == idUsuarioEliminar, ct);
        if (usuario is null)
            throw new BusinessException("USER_NOT_FOUND", "El usuario no existe.");

        // EquipoMiembros tiene FK con OnDelete=NoAction (multi-cascade-path issue de SQL Server),
        // así que limpiamos manualmente sus membresías antes de eliminar el usuario.
        var membresias = await _context.EquiposMiembros
            .Where(em => em.IdUsuario == idUsuarioEliminar)
            .ToListAsync(ct);
        if (membresias.Count > 0)
            _context.EquiposMiembros.RemoveRange(membresias);

        // Refresh tokens y tokens de reset se eliminan en cascada (configurado en EF).
        // El líder de algún equipo se setea a NULL automáticamente (FK Equipo.IdLider → SetNull).
        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync(ct);
    }
}
