using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Application.Equipos.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Equipos.UseCases;

public class ObtenerEquipoUseCase
{
    private readonly ICdtDbContext _context;

    public ObtenerEquipoUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<EquipoDetalleDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var equipo = await _context.Equipos
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EquipoDetalleDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                IdEquipoPadre = e.IdEquipoPadre,
                NombreEquipoPadre = e.EquipoPadre != null ? e.EquipoPadre.Nombre : null,
                IdLider = e.IdLider,
                NombreLider = e.Lider != null ? e.Lider.Nombre : null,
                CorreoLider = e.Lider != null ? e.Lider.Correo : null,
                FechaCreacion = e.FechaCreacion,
                Miembros = e.Miembros
                    .OrderBy(m => m.Usuario.Nombre)
                    .Select(m => new MiembroDto
                    {
                        Id = m.Usuario.Id,
                        Nombre = m.Usuario.Nombre,
                        Correo = m.Usuario.Correo,
                        RolGlobal = m.Usuario.RolGlobal,
                        FechaAgregado = m.FechaAgregado,
                    })
                    .ToList(),
                SubEquipos = e.SubEquipos
                    .OrderBy(se => se.Nombre)
                    .Select(se => new EquipoListItemDto
                    {
                        Id = se.Id,
                        Nombre = se.Nombre,
                        IdEquipoPadre = se.IdEquipoPadre,
                        NombreEquipoPadre = e.Nombre,
                        IdLider = se.IdLider,
                        NombreLider = se.Lider != null ? se.Lider.Nombre : null,
                        CorreoLider = se.Lider != null ? se.Lider.Correo : null,
                        TotalMiembros = se.Miembros.Count,
                        TotalSubEquipos = se.SubEquipos.Count,
                        FechaCreacion = se.FechaCreacion,
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(ct);

        if (equipo is null)
            throw new BusinessException("EQUIPO_NOT_FOUND", "El equipo no existe.");

        return equipo;
    }
}
