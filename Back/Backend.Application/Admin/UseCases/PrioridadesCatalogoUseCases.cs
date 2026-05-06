using Backend.Application.Admin.Commands;
using Backend.Application.Admin.DTOs;
using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Domain.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Admin.UseCases;

/// <summary>CRUD de Prioridades (catálogo parametrizable).</summary>
public class PrioridadesCatalogoUseCases
{
    private readonly ICdtDbContext _context;

    public PrioridadesCatalogoUseCases(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<List<CatalogoItemDto>> ListarAsync(CancellationToken ct = default) =>
        await _context.Prioridades
            .AsNoTracking()
            .OrderBy(p => p.Orden).ThenBy(p => p.Nombre)
            .Select(p => new CatalogoItemDto { Id = p.Id, Nombre = p.Nombre, Color = p.Color, Orden = p.Orden })
            .ToListAsync(ct);

    public async Task<CatalogoItemDto> CrearAsync(CrearCatalogoItemCommand cmd, CancellationToken ct = default)
    {
        var entity = new PrioridadEntity { Nombre = cmd.Nombre.Trim(), Color = cmd.Color, Orden = cmd.Orden };
        _context.Prioridades.Add(entity);
        try { await _context.SaveChangesAsync(ct); }
        catch (DbUpdateException) { throw new BusinessException("CATALOGO_DUPLICADO", "Ya existe una prioridad con ese nombre."); }
        return new CatalogoItemDto { Id = entity.Id, Nombre = entity.Nombre, Color = entity.Color, Orden = entity.Orden };
    }

    public async Task<CatalogoItemDto> ActualizarAsync(int id, ActualizarCatalogoItemCommand cmd, CancellationToken ct = default)
    {
        var entity = await _context.Prioridades.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new BusinessException("PRIORIDAD_NOT_FOUND", "La prioridad no existe.");

        entity.Nombre = cmd.Nombre.Trim();
        entity.Color = cmd.Color;
        entity.Orden = cmd.Orden;

        try { await _context.SaveChangesAsync(ct); }
        catch (DbUpdateException) { throw new BusinessException("CATALOGO_DUPLICADO", "Ya existe una prioridad con ese nombre."); }
        return new CatalogoItemDto { Id = entity.Id, Nombre = entity.Nombre, Color = entity.Color, Orden = entity.Orden };
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.Prioridades.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new BusinessException("PRIORIDAD_NOT_FOUND", "La prioridad no existe.");
        _context.Prioridades.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
