using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Application.Listas.DTOs;
using Backend.Application.Listas.Queries;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Listas.UseCases;

public class ListarListasUseCase
{
    private readonly ICdtDbContext _context;

    public ListarListasUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<List<ListaDto>> ExecuteAsync(ListarListasQuery query, CancellationToken ct = default)
    {
        var proyectoExiste = await _context.Proyectos.AnyAsync(p => p.Id == query.IdProyecto, ct);
        if (!proyectoExiste)
            throw new BusinessException("PROYECTO_NOT_FOUND", "El proyecto no existe.");

        return await _context.Listas
            .AsNoTracking()
            .Where(l => l.IdProyecto == query.IdProyecto)
            .OrderBy(l => l.Orden)
            .ThenBy(l => l.Id)
            .Select(l => new ListaDto
            {
                Id = l.Id,
                Nombre = l.Nombre,
                Color = l.Color,
                Orden = l.Orden,
                IdProyecto = l.IdProyecto,
                NombreProyecto = l.Proyecto.Nombre,
                IdCreador = l.IdCreador,
                NombreCreador = l.Creador != null ? l.Creador.Nombre : null,
                FechaCreacion = l.FechaCreacion,
                FechaModificacion = l.FechaModificacion,
            })
            .ToListAsync(ct);
    }
}
