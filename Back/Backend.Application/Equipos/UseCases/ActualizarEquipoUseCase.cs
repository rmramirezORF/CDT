using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Application.Equipos.Commands;
using Backend.Application.Equipos.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Equipos.UseCases;

public class ActualizarEquipoUseCase
{
    private readonly ICdtDbContext _context;

    public ActualizarEquipoUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<EquipoListItemDto> ExecuteAsync(int id, ActualizarEquipoCommand cmd, CancellationToken ct = default)
    {
        var equipo = await _context.Equipos.FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new BusinessException("EQUIPO_NOT_FOUND", "El equipo no existe.");

        // Validar padre: existe + no-ciclo (no puede ser hijo de uno de sus descendientes)
        if (cmd.IdEquipoPadre.HasValue)
        {
            if (cmd.IdEquipoPadre.Value == id)
                throw new BusinessException("CICLO_JERARQUIA", "Un equipo no puede ser padre de sí mismo.");

            var padreExiste = await _context.Equipos.AnyAsync(e => e.Id == cmd.IdEquipoPadre.Value, ct);
            if (!padreExiste)
                throw new BusinessException("EQUIPO_PADRE_NOT_FOUND", "El equipo padre no existe.");

            if (await CrearíaCiclo(id, cmd.IdEquipoPadre.Value, ct))
                throw new BusinessException("CICLO_JERARQUIA",
                    "No se puede asignar ese padre porque crearía un ciclo en la jerarquía.");
        }

        // Validar líder
        if (cmd.IdLider.HasValue)
        {
            var liderExiste = await _context.Usuarios.AnyAsync(u => u.Id == cmd.IdLider.Value, ct);
            if (!liderExiste)
                throw new BusinessException("LIDER_NOT_FOUND", "El usuario asignado como líder no existe.");
        }

        equipo.Nombre = cmd.Nombre.Trim();
        equipo.IdEquipoPadre = cmd.IdEquipoPadre;
        equipo.IdLider = cmd.IdLider;

        await _context.SaveChangesAsync(ct);

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
                TotalMiembros = e.Miembros.Count,
                TotalSubEquipos = e.SubEquipos.Count,
                FechaCreacion = e.FechaCreacion,
            })
            .FirstAsync(ct);
    }

    /// <summary>
    /// ¿Asignar `nuevoPadreId` como padre de `equipoId` crearía un ciclo?
    /// Ocurre si `nuevoPadreId` es descendiente de `equipoId`.
    /// </summary>
    private async Task<bool> CrearíaCiclo(int equipoId, int nuevoPadreId, CancellationToken ct)
    {
        // Recorremos hacia arriba desde nuevoPadreId. Si en algún momento toco equipoId, hay ciclo.
        var actual = (int?)nuevoPadreId;
        var visitados = new HashSet<int>();

        while (actual.HasValue)
        {
            if (actual.Value == equipoId) return true;
            if (!visitados.Add(actual.Value)) return true; // protección anti-loop infinito

            actual = await _context.Equipos
                .Where(e => e.Id == actual.Value)
                .Select(e => e.IdEquipoPadre)
                .FirstOrDefaultAsync(ct);
        }

        return false;
    }
}
