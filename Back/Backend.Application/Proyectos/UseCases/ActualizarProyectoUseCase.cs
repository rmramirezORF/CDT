using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Application.Proyectos.Commands;
using Backend.Application.Proyectos.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Proyectos.UseCases;

public class ActualizarProyectoUseCase
{
    private readonly ICdtDbContext _context;

    public ActualizarProyectoUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<ProyectoDto> ExecuteAsync(int id, ActualizarProyectoCommand cmd, CancellationToken ct = default)
    {
        var proyecto = await _context.Proyectos.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new BusinessException("PROYECTO_NOT_FOUND", "El proyecto no existe.");

        var equipoExiste = await _context.Equipos.AnyAsync(e => e.Id == cmd.IdEquipo, ct);
        if (!equipoExiste)
            throw new BusinessException("EQUIPO_NOT_FOUND", "El equipo no existe.");

        proyecto.Nombre = cmd.Nombre.Trim();
        proyecto.Descripcion = cmd.Descripcion?.Trim();
        proyecto.IdEquipo = cmd.IdEquipo;
        proyecto.FechaModificacion = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return await _context.Proyectos
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
            .FirstAsync(ct);
    }
}
