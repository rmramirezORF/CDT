using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Tareas.UseCases;

public class EliminarTareaUseCase
{
    private readonly ICdtDbContext _context;

    public EliminarTareaUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new BusinessException("TAREA_NOT_FOUND", "La tarea no existe.");
        _context.Tareas.Remove(tarea);
        await _context.SaveChangesAsync(ct);
    }
}
