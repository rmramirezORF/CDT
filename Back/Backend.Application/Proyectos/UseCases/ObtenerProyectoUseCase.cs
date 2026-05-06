using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Application.Proyectos.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Proyectos.UseCases;

public class ObtenerProyectoUseCase
{
    private readonly ICdtDbContext _context;

    public ObtenerProyectoUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<ProyectoDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var proyecto = await _context.Proyectos
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProyectoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Clave = p.Clave,
                Descripcion = p.Descripcion,
                IdEquipo = p.IdEquipo,
                NombreEquipo = p.Equipo.Nombre,
                IdCreador = p.IdCreador,
                NombreCreador = p.Creador != null ? p.Creador.Nombre : null,
                FechaCreacion = p.FechaCreacion,
                FechaModificacion = p.FechaModificacion,
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new BusinessException("PROYECTO_NOT_FOUND", "El proyecto no existe.");

        return proyecto;
    }
}
