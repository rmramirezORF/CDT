using System.Linq.Expressions;
using Backend.Application.Tareas.DTOs;
using Backend.Domain.Entities.Tareas;

namespace Backend.Application.Tareas.UseCases;

internal static class TareaProjection
{
    /// <summary>
    /// Proyección compartida TareaEntity → TareaDto. Se usa en todos los use cases
    /// para asegurar consistencia y evitar consultas N+1.
    /// </summary>
    public static readonly Expression<Func<TareaEntity, TareaDto>> ToDto = t => new TareaDto
    {
        Id = t.Id,
        NumeroEnProyecto = t.NumeroEnProyecto,
        Clave = t.Proyecto.Clave + "-" + t.NumeroEnProyecto.ToString(),
        Titulo = t.Titulo,
        Descripcion = t.Descripcion,
        IdProyecto = t.IdProyecto,
        NombreProyecto = t.Proyecto.Nombre,
        ClaveProyecto = t.Proyecto.Clave,
        IdLista = t.IdLista,
        NombreLista = t.Lista.Nombre,
        IdTipoActividad = t.IdTipoActividad,
        NombreTipoActividad = t.TipoActividad != null ? t.TipoActividad.Nombre : null,
        ColorTipoActividad = t.TipoActividad != null ? t.TipoActividad.Color : null,
        IdEstado = t.IdEstado,
        NombreEstado = t.Estado != null ? t.Estado.Nombre : null,
        ColorEstado = t.Estado != null ? t.Estado.Color : null,
        IdPrioridad = t.IdPrioridad,
        NombrePrioridad = t.Prioridad != null ? t.Prioridad.Nombre : null,
        ColorPrioridad = t.Prioridad != null ? t.Prioridad.Color : null,
        IdResponsable = t.IdResponsable,
        NombreResponsable = t.Responsable != null ? t.Responsable.Nombre : null,
        IdInformador = t.IdInformador,
        NombreInformador = t.Informador != null ? t.Informador.Nombre : null,
        FechaVencimiento = t.FechaVencimiento,
        Orden = t.Orden,
        FechaCreacion = t.FechaCreacion,
        FechaModificacion = t.FechaModificacion,
    };
}
