using Backend.Application.Common.Persistence;
using Backend.Application.Equipos.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Equipos.UseCases;

public class ListarEquiposUseCase
{
    private readonly ICdtDbContext _context;

    public ListarEquiposUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<List<EquipoListItemDto>> ExecuteAsync(CancellationToken ct = default) =>
        await _context.Equipos
            .AsNoTracking()
            .OrderBy(e => e.Nombre)
            .Select(e => new EquipoListItemDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                IdEquipoPadre = e.IdEquipoPadre,
                NombreEquipoPadre = e.EquipoPadre != null ? e.EquipoPadre.Nombre : null,
                IdLider = e.IdLider,
                NombreLider = e.Lider != null ? e.Lider.Nombre : null,
                CorreoLider = e.Lider != null ? e.Lider.Correo : null,
                TotalMiembros = e.Miembros.Count,
                TotalSubEquipos = e.SubEquipos.Count,
                FechaCreacion = e.FechaCreacion,
            })
            .ToListAsync(ct);
}
