namespace Backend.Application.Listas.Queries;

public class ListarListasQuery
{
    /// <summary>
    /// Obligatorio: las listas siempre se consultan dentro de un proyecto.
    /// </summary>
    public int IdProyecto { get; set; }
}
