using Backend.Application.Admin.Commands;
using Backend.Application.Admin.DTOs;
using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Domain.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Admin.UseCases;

/// <summary>CRUD de Tipos de Actividad (catálogo parametrizable, estilo Jira).</summary>
public class TiposActividadCatalogoUseCases
{
    private readonly ICdtDbContext _context;

    public TiposActividadCatalogoUseCases(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<List<CatalogoItemDto>> ListarAsync(CancellationToken ct = default) =>
        await _context.TiposActividad
            .AsNoTracking()
            .OrderBy(e => e.Orden).ThenBy(e => e.Nombre)
            .Select(e => new CatalogoItemDto { Id = e.Id, Nombre = e.Nombre, Color = e.Color, Orden = e.Orden })
            .ToListAsync(ct);

    public async Task<CatalogoItemDto> CrearAsync(CrearCatalogoItemCommand cmd, CancellationToken ct = default)
    {
        var entity = new TipoActividadEntity { Nombre = cmd.Nombre.Trim(), Color = cmd.Color, Orden = cmd.Orden };
        _context.TiposActividad.Add(entity);
        try { await _context.SaveChangesAsync(ct); }
        catch (DbUpdateException) { throw new BusinessException("CATALOGO_DUPLICADO", "Ya existe un tipo de actividad con ese nombre."); }
        return new CatalogoItemDto { Id = entity.Id, Nombre = entity.Nombre, Color = entity.Color, Orden = entity.Orden };
    }

    public async Task<CatalogoItemDto> ActualizarAsync(int id, ActualizarCatalogoItemCommand cmd, CancellationToken ct = default)
    {
        var entity = await _context.TiposActividad.FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new BusinessException("TIPO_ACTIVIDAD_NOT_FOUND", "El tipo de actividad no existe.");

        entity.Nombre = cmd.Nombre.Trim();
        entity.Color = cmd.Color;
        entity.Orden = cmd.Orden;

        try { await _context.SaveChangesAsync(ct); }
        catch (DbUpdateException) { throw new BusinessException("CATALOGO_DUPLICADO", "Ya existe un tipo de actividad con ese nombre."); }
        return new CatalogoItemDto { Id = entity.Id, Nombre = entity.Nombre, Color = entity.Color, Orden = entity.Orden };
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.TiposActividad.FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new BusinessException("TIPO_ACTIVIDAD_NOT_FOUND", "El tipo de actividad no existe.");
        _context.TiposActividad.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
