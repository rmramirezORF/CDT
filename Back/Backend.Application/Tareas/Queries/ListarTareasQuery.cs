namespace Backend.Application.Tareas.Queries;

public class ListarTareasQuery
{
    /// <summary>Filtrar por proyecto (vista global del proyecto).</summary>
    public int? IdProyecto { get; set; }

    /// <summary>Filtrar por lista (columna Kanban).</summary>
    public int? IdLista { get; set; }

    /// <summary>Filtrar por responsable.</summary>
    public int? IdResponsable { get; set; }

    /// <summary>Filtrar por estado.</summary>
    public int? IdEstado { get; set; }

    /// <summary>Filtrar por prioridad.</summary>
    public int? IdPrioridad { get; set; }

    /// <summary>Búsqueda por título o clave (ej: "ANI-1").</summary>
    public string? Q { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
