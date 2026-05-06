using Backend.Application.Admin.DTOs;
using Backend.Application.Admin.Queries;
using Backend.Application.Common.DTOs;
using Backend.Application.Common.Extensions;
using Backend.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Admin.UseCases;

public class ListarUsuariosUseCase
{
    private readonly ICdtDbContext _context;

    public ListarUsuariosUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<(List<UsuarioListItemDto> Items, Pagination Pagination)> ExecuteAsync(
        ListarUsuariosQuery query,
        CancellationToken ct = default)
    {
        var page     = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var q        = query.Q?.Trim().ToLowerInvariant();

        var baseQuery = _context.Usuarios
            .AsNoTracking()
            .WhereIf(!string.IsNullOrEmpty(q), u => u.Correo.Contains(q!) || u.Nombre.ToLower().Contains(q!))
            .WhereIf(!string.IsNullOrEmpty(query.Rol), u => u.RolGlobal == query.Rol!)
            .WhereIf(query.Estado.HasValue, u => u.Estado == query.Estado!.Value);

        var total = await baseQuery.LongCountAsync(ct);

        var items = await baseQuery
            .OrderBy(u => u.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UsuarioListItemDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Correo = u.Correo,
                RolGlobal = u.RolGlobal,
                Estado = u.Estado,
                FechaCreacion = u.FechaCreacion,
                FechaConfirmacionEmail = u.FechaConfirmacionEmail,
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
