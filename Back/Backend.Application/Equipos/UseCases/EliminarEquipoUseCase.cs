using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Equipos.UseCases;

public class EliminarEquipoUseCase
{
    private readonly ICdtDbContext _context;

    public EliminarEquipoUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var equipo = await _context.Equipos
            .Include(e => e.Miembros)
            .Include(e => e.SubEquipos)
            .FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new BusinessException("EQUIPO_NOT_FOUND", "El equipo no existe.");

        if (equipo.SubEquipos.Count > 0)
            throw new BusinessException(
                "EQUIPO_TIENE_SUBEQUIPOS",
                $"No se puede eliminar: el equipo tiene {equipo.SubEquipos.Count} sub-equipo(s). Reasigná o eliminálos primero.");

        if (equipo.Miembros.Count > 0)
            throw new BusinessException(
                "EQUIPO_TIENE_MIEMBROS",
                $"No se puede eliminar: el equipo tiene {equipo.Miembros.Count} miembro(s). Quitalos primero.");

        _context.Equipos.Remove(equipo);
        await _context.SaveChangesAsync(ct);
    }
}
