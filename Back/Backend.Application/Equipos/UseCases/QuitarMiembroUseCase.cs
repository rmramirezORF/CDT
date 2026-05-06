using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Equipos.UseCases;

public class QuitarMiembroUseCase
{
    private readonly ICdtDbContext _context;

    public QuitarMiembroUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(int idEquipo, int idUsuario, CancellationToken ct = default)
    {
        var miembro = await _context.EquiposMiembros
            .FirstOrDefaultAsync(m => m.IdEquipo == idEquipo && m.IdUsuario == idUsuario, ct)
            ?? throw new BusinessException("MIEMBRO_NOT_FOUND", "Ese usuario no es miembro del equipo.");

        _context.EquiposMiembros.Remove(miembro);
        await _context.SaveChangesAsync(ct);
    }
}
