using Backend.Application.Common.DTOs;
using Backend.Application.Common.Extensions;
using Backend.Application.Common.Persistence;
using Backend.Application.Tareas.DTOs;
using Backend.Application.Tareas.Queries;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Tareas.UseCases;

public class ListarTareasUseCase
{
    private readonly ICdtDbContext _context;

    public ListarTareasUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<(List<TareaDto> Items, Pagination Pagination)> ExecuteAsync(
        ListarTareasQuery query,
        CancellationToken ct = default)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var q = query.Q?.Trim().ToLowerInvariant();

        var baseQuery = _context.Tareas
            .AsNoTracking()
            .WhereIf(query.IdProyecto.HasValue, t => t.IdProyecto == query.IdProyecto!.Value)
            .WhereIf(query.IdLista.HasValue, t => t.IdLista == query.IdLista!.Value)
            .WhereIf(query.IdResponsable.HasValue, t => t.IdResponsable == query.IdResponsable!.Value)
            .WhereIf(query.IdEstado.HasValue, t => t.IdEstado == query.IdEstado!.Value)
            .WhereIf(query.IdPrioridad.HasValue, t => t.IdPrioridad == query.IdPrioridad!.Value)
            .WhereIf(!string.IsNullOrEmpty(q),
                t => t.Titulo.ToLower().Contains(q!)
                  || (t.Proyecto.Clave + "-" + t.NumeroEnProyecto.ToString()).ToLower().Contains(q!));

        var total = await baseQuery.LongCountAsync(ct);

        var items = await baseQuery
            .OrderBy(t => t.IdLista)
            .ThenBy(t => t.Orden)
            .ThenByDescending(t => t.FechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(TareaProjection.ToDto)
            .ToListAsync(ct);

        return (items, new Pagination
        {
            Page = page,
            PageSize = pageSize,
            TotalRecords = total,
        });
    }
}
