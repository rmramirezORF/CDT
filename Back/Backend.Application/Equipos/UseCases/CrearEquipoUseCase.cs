using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Application.Equipos.Commands;
using Backend.Application.Equipos.DTOs;
using Backend.Domain.Entities.Equipos;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Equipos.UseCases;

public class CrearEquipoUseCase
{
    private readonly ICdtDbContext _context;

    public CrearEquipoUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<EquipoListItemDto> ExecuteAsync(CrearEquipoCommand cmd, CancellationToken ct = default)
    {
        // Validar padre existe
        if (cmd.IdEquipoPadre.HasValue)
        {
            var padreExiste = await _context.Equipos.AnyAsync(e => e.Id == cmd.IdEquipoPadre.Value, ct);
            if (!padreExiste)
                throw new BusinessException("EQUIPO_PADRE_NOT_FOUND", "El equipo padre no existe.");
        }

        // Validar líder existe (si se asigna)
        if (cmd.IdLider.HasValue)
        {
            var liderExiste = await _context.Usuarios.AnyAsync(u => u.Id == cmd.IdLider.Value, ct);
            if (!liderExiste)
                throw new BusinessException("LIDER_NOT_FOUND", "El usuario asignado como líder no existe.");
        }

        var equipo = new EquipoEntity
        {
            Nombre = cmd.Nombre.Trim(),
            IdEquipoPadre = cmd.IdEquipoPadre,
            IdLider = cmd.IdLider,
        };

        _context.Equipos.Add(equipo);
        await _context.SaveChangesAsync(ct);

        // Volver a leer con joins para devolver DTO completo
        return await _context.Equipos
            .AsNoTracking()
            .Where(e => e.Id == equipo.Id)
            .Select(e => new EquipoListItemDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                IdEquipoPadre = e.IdEquipoPadre,
                NombreEquipoPadre = e.EquipoPadre != null ? e.EquipoPadre.Nombre : null,
                IdLider = e.IdLider,
                NombreLider = e.Lider != null ? e.Lider.Nombre : null,
                CorreoLider = e.Lider != null ? e.Lider.Correo : null,
                TotalMiembros = 0,
                TotalSubEquipos = 0,
                FechaCreacion = e.FechaCreacion,
            })
            .FirstAsync(ct);
    }
}
