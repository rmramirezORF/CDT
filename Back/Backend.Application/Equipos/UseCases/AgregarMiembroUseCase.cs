using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Application.Equipos.Commands;
using Backend.Application.Equipos.DTOs;
using Backend.Domain.Entities.Equipos;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Equipos.UseCases;

public class AgregarMiembroUseCase
{
    private readonly ICdtDbContext _context;

    public AgregarMiembroUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<MiembroDto> ExecuteAsync(int idEquipo, AgregarMiembroCommand cmd, CancellationToken ct = default)
    {
        var equipoExiste = await _context.Equipos.AnyAsync(e => e.Id == idEquipo, ct);
        if (!equipoExiste)
            throw new BusinessException("EQUIPO_NOT_FOUND", "El equipo no existe.");

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == cmd.IdUsuario, ct)
            ?? throw new BusinessException("USER_NOT_FOUND", "El usuario no existe.");

        if (!usuario.Estado)
            throw new BusinessException("USER_INACTIVE", "El usuario está inactivo. Activalo antes de asignarlo.");

        var yaEsMiembro = await _context.EquiposMiembros
            .AnyAsync(m => m.IdEquipo == idEquipo && m.IdUsuario == cmd.IdUsuario, ct);
        if (yaEsMiembro)
            throw new BusinessException("MIEMBRO_DUPLICADO", "El usuario ya es miembro de este equipo.");

        var miembro = new EquipoMiembroEntity
        {
            IdEquipo = idEquipo,
            IdUsuario = cmd.IdUsuario,
            FechaAgregado = DateTime.UtcNow,
        };
        _context.EquiposMiembros.Add(miembro);
        await _context.SaveChangesAsync(ct);

        return new MiembroDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            RolGlobal = usuario.RolGlobal,
            FechaAgregado = miembro.FechaAgregado,
        };
    }
}
