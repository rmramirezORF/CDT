using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Proyectos.UseCases;

public class EliminarProyectoUseCase
{
    private readonly ICdtDbContext _context;

    public EliminarProyectoUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var proyecto = await _context.Proyectos.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new BusinessException("PROYECTO_NOT_FOUND", "El proyecto no existe.");

        // V2: validar que no tenga listas/tareas (cuando esos modulos existan).
        _context.Proyectos.Remove(proyecto);
        await _context.SaveChangesAsync(ct);
    }
}
