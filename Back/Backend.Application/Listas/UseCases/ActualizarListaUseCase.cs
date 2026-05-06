using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Application.Listas.Commands;
using Backend.Application.Listas.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Listas.UseCases;

public class ActualizarListaUseCase
{
    private readonly ICdtDbContext _context;

    public ActualizarListaUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<ListaDto> ExecuteAsync(int id, ActualizarListaCommand cmd, CancellationToken ct = default)
    {
        var lista = await _context.Listas.FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new BusinessException("LISTA_NOT_FOUND", "La lista no existe.");

        lista.Nombre = cmd.Nombre.Trim();
        lista.Color = string.IsNullOrWhiteSpace(cmd.Color) ? null : cmd.Color.Trim();
        lista.FechaModificacion = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return await _context.Listas
            .AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new ListaDto
            {
                Id = l.Id,
                Nombre = l.Nombre,
                Color = l.Color,
                Orden = l.Orden,
                IdProyecto = l.IdProyecto,
                NombreProyecto = l.Proyecto.Nombre,
                IdCreador = l.IdCreador,
                NombreCreador = l.Creador != null ? l.Creador.Nombre : null,
                FechaCreacion = l.FechaCreacion,
                FechaModificacion = l.FechaModificacion,
            })
            .FirstAsync(ct);
    }
}
