namespace Backend.Application.Proyectos.Queries;

public class ListarProyectosQuery
{
    /// <summary>Búsqueda en el nombre del proyecto.</summary>
    public string? Q { get; set; }

    /// <summary>Filtrar por equipo específico.</summary>
    public int? IdEquipo { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
