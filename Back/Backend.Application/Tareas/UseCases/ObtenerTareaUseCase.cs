using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Application.Tareas.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Tareas.UseCases;

public class ObtenerTareaUseCase
{
    private readonly ICdtDbContext _context;

    public ObtenerTareaUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<TareaDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        return await _context.Tareas
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(TareaProjection.ToDto)
            .FirstOrDefaultAsync(ct)
            ?? throw new BusinessException("TAREA_NOT_FOUND", "La tarea no existe.");
    }
}
