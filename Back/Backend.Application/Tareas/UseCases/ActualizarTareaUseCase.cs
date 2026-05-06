using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Application.Tareas.Commands;
using Backend.Application.Tareas.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Tareas.UseCases;

public class ActualizarTareaUseCase
{
    private readonly ICdtDbContext _context;

    public ActualizarTareaUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<TareaDto> ExecuteAsync(int id, ActualizarTareaCommand cmd, CancellationToken ct = default)
    {
        var tarea = await _context.Tareas.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new BusinessException("TAREA_NOT_FOUND", "La tarea no existe.");

        // Cambio de lista: validar que pertenezca al mismo proyecto y reasignar orden al final.
        if (cmd.IdLista.HasValue && cmd.IdLista.Value != tarea.IdLista)
        {
            var listaDestino = await _context.Listas.FirstOrDefaultAsync(l => l.Id == cmd.IdLista.Value, ct)
                ?? throw new BusinessException("LISTA_NOT_FOUND", "La lista de destino no existe.");

            if (listaDestino.IdProyecto != tarea.IdProyecto)
                throw new BusinessException("LISTA_OTRO_PROYECTO", "La lista de destino pertenece a otro proyecto.");

            var maxOrden = await _context.Tareas
                .Where(t => t.IdLista == listaDestino.Id)
                .MaxAsync(t => (int?)t.Orden, ct) ?? -1;

            tarea.IdLista = listaDestino.Id;
            tarea.Orden = maxOrden + 1;
        }

        // Validar catálogos si se enviaron.
        if (cmd.IdTipoActividad.HasValue && !await _context.TiposActividad.AnyAsync(x => x.Id == cmd.IdTipoActividad.Value, ct))
            throw new BusinessException("TIPO_ACTIVIDAD_NOT_FOUND", "El tipo de actividad no existe.");
        if (cmd.IdEstado.HasValue && !await _context.Estados.AnyAsync(x => x.Id == cmd.IdEstado.Value, ct))
            throw new BusinessException("ESTADO_NOT_FOUND", "El estado no existe.");
        if (cmd.IdPrioridad.HasValue && !await _context.Prioridades.AnyAsync(x => x.Id == cmd.IdPrioridad.Value, ct))
            throw new BusinessException("PRIORIDAD_NOT_FOUND", "La prioridad no existe.");
        if (cmd.IdResponsable.HasValue && !await _context.Usuarios.AnyAsync(u => u.Id == cmd.IdResponsable.Value, ct))
            throw new BusinessException("RESPONSABLE_NOT_FOUND", "El responsable no existe.");

        tarea.Titulo = cmd.Titulo.Trim();
        tarea.Descripcion = cmd.Descripcion?.Trim();
        tarea.IdTipoActividad = cmd.IdTipoActividad;
        tarea.IdEstado = cmd.IdEstado;
        tarea.IdPrioridad = cmd.IdPrioridad;
        tarea.IdResponsable = cmd.IdResponsable;
        tarea.FechaVencimiento = cmd.FechaVencimiento;
        tarea.FechaModificacion = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return await _context.Tareas
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(TareaProjection.ToDto)
            .FirstAsync(ct);
    }
}
