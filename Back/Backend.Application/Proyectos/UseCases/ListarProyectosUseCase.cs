using Backend.Application.Common.DTOs;
using Backend.Application.Common.Extensions;
using Backend.Application.Common.Persistence;
using Backend.Application.Proyectos.DTOs;
using Backend.Application.Proyectos.Queries;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Proyectos.UseCases;

public class ListarProyectosUseCase
{
    private readonly ICdtDbContext _context;

    public ListarProyectosUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<(List<ProyectoDto> Items, Pagination Pagination)> ExecuteAsync(
        ListarProyectosQuery query,
        CancellationToken ct = default)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var q = query.Q?.Trim().ToLowerInvariant();

        var baseQuery = _context.Proyectos
            .AsNoTracking()
            .WhereIf(!string.IsNullOrEmpty(q),
                p => p.Nombre.ToLower().Contains(q!) || p.Clave.ToLower().Contains(q!))
            .WhereIf(query.IdEquipo.HasValue, p => p.IdEquipo == query.IdEquipo!.Value);

        var total = await baseQuery.LongCountAsync(ct);

        var items = await baseQuery
            .OrderByDescending(p => p.FechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
            .ToListAsync(ct);

        return (items, new Pagination
        {
            Page = page,
            PageSize = pageSize,
            TotalRecords = total,
        });
    }
}
