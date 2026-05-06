using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Application.Proyectos.Commands;
using Backend.Application.Proyectos.DTOs;
using Backend.Domain.Entities.Proyectos;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Proyectos.UseCases;

public class CrearProyectoUseCase
{
    private readonly ICdtDbContext _context;

    public CrearProyectoUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<ProyectoDto> ExecuteAsync(CrearProyectoCommand cmd, int idCreador, CancellationToken ct = default)
    {
        var equipoExiste = await _context.Equipos.AnyAsync(e => e.Id == cmd.IdEquipo, ct);
        if (!equipoExiste)
            throw new BusinessException("EQUIPO_NOT_FOUND", "El equipo no existe.");

        var clave = cmd.Clave.Trim().ToUpperInvariant();
        var claveEnUso = await _context.Proyectos.AnyAsync(p => p.Clave == clave, ct);
        if (claveEnUso)
            throw new BusinessException("CLAVE_DUPLICADA", $"Ya existe un proyecto con la clave '{clave}'.");

        var proyecto = new ProyectoEntity
        {
            Nombre = cmd.Nombre.Trim(),
            Clave = clave,
            Descripcion = cmd.Descripcion?.Trim(),
            IdEquipo = cmd.IdEquipo,
            IdCreador = idCreador,
        };

        _context.Proyectos.Add(proyecto);
        await _context.SaveChangesAsync(ct);

        return await _context.Proyectos
            .AsNoTracking()
            .Where(p => p.Id == proyecto.Id)
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
