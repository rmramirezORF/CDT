using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Application.Tareas.Commands;
using Backend.Application.Tareas.DTOs;
using Backend.Domain.Entities.Tareas;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Tareas.UseCases;

public class CrearTareaUseCase
{
    private readonly ICdtDbContext _context;

    public CrearTareaUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<TareaDto> ExecuteAsync(CrearTareaCommand cmd, int idInformador, CancellationToken ct = default)
    {
        var lista = await _context.Listas.FirstOrDefaultAsync(l => l.Id == cmd.IdLista, ct)
            ?? throw new BusinessException("LISTA_NOT_FOUND", "La lista no existe.");

        // Bloqueamos el proyecto para incrementar UltimoNumeroTarea de forma segura.
        var proyecto = await _context.Proyectos.FirstOrDefaultAsync(p => p.Id == lista.IdProyecto, ct)
            ?? throw new BusinessException("PROYECTO_NOT_FOUND", "El proyecto no existe.");

        // Validar catálogos opcionales si se enviaron.
        await ValidarCatalogos(cmd.IdTipoActividad, cmd.IdEstado, cmd.IdPrioridad, ct);
        if (cmd.IdResponsable.HasValue)
        {
            var existe = await _context.Usuarios.AnyAsync(u => u.Id == cmd.IdResponsable.Value, ct);
            if (!existe) throw new BusinessException("RESPONSABLE_NOT_FOUND", "El responsable no existe.");
        }

        proyecto.UltimoNumeroTarea++;

        // Orden: al final de la lista destino.
        var maxOrden = await _context.Tareas
            .Where(t => t.IdLista == cmd.IdLista)
            .MaxAsync(t => (int?)t.Orden, ct) ?? -1;

        var tarea = new TareaEntity
        {
            Titulo = cmd.Titulo.Trim(),
            Descripcion = cmd.Descripcion?.Trim(),
            IdProyecto = proyecto.Id,
            IdLista = cmd.IdLista,
            IdTipoActividad = cmd.IdTipoActividad,
            IdEstado = cmd.IdEstado,
            IdPrioridad = cmd.IdPrioridad,
            IdResponsable = cmd.IdResponsable,
            IdInformador = idInformador,
            FechaVencimiento = cmd.FechaVencimiento,
            NumeroEnProyecto = proyecto.UltimoNumeroTarea,
            Orden = maxOrden + 1,
        };

        _context.Tareas.Add(tarea);
        await _context.SaveChangesAsync(ct);

        return await _context.Tareas
            .AsNoTracking()
            .Where(t => t.Id == tarea.Id)
            .Select(TareaProjection.ToDto)
            .FirstAsync(ct);
    }

    private async Task ValidarCatalogos(int? idTipo, int? idEstado, int? idPrioridad, CancellationToken ct)
    {
        if (idTipo.HasValue && !await _context.TiposActividad.AnyAsync(x => x.Id == idTipo.Value, ct))
            throw new BusinessException("TIPO_ACTIVIDAD_NOT_FOUND", "El tipo de actividad no existe.");
        if (idEstado.HasValue && !await _context.Estados.AnyAsync(x => x.Id == idEstado.Value, ct))
            throw new BusinessException("ESTADO_NOT_FOUND", "El estado no existe.");
        if (idPrioridad.HasValue && !await _context.Prioridades.AnyAsync(x => x.Id == idPrioridad.Value, ct))
            throw new BusinessException("PRIORIDAD_NOT_FOUND", "La prioridad no existe.");
    }
}
