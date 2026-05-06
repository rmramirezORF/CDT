using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Application.Listas.Commands;
using Backend.Application.Listas.DTOs;
using Backend.Domain.Entities.Proyectos;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Listas.UseCases;

public class CrearListaUseCase
{
    private readonly ICdtDbContext _context;

    public CrearListaUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<ListaDto> ExecuteAsync(CrearListaCommand cmd, int idCreador, CancellationToken ct = default)
    {
        var proyectoExiste = await _context.Proyectos.AnyAsync(p => p.Id == cmd.IdProyecto, ct);
        if (!proyectoExiste)
            throw new BusinessException("PROYECTO_NOT_FOUND", "El proyecto no existe.");

        // Si no se envía orden, lo posicionamos al final.
        var orden = cmd.Orden;
        if (orden is null)
        {
            var max = await _context.Listas
                .Where(l => l.IdProyecto == cmd.IdProyecto)
                .MaxAsync(l => (int?)l.Orden, ct) ?? -1;
            orden = max + 1;
        }

        var lista = new ListaEntity
        {
            Nombre = cmd.Nombre.Trim(),
            Color = string.IsNullOrWhiteSpace(cmd.Color) ? null : cmd.Color.Trim(),
            Orden = orden.Value,
            IdProyecto = cmd.IdProyecto,
            IdCreador = idCreador,
        };

        _context.Listas.Add(lista);
        await _context.SaveChangesAsync(ct);

        return await _context.Listas
            .AsNoTracking()
            .Where(l => l.Id == lista.Id)
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
            .FirstAsync(ct);
    }
}
