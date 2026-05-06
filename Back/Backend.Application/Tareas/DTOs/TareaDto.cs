namespace Backend.Application.Tareas.DTOs;

public class TareaDto
{
    public int Id { get; set; }

    /// <summary>Clave compuesta tipo Jira: "{ClaveProyecto}-{NumeroEnProyecto}", ej: "ANI-12".</summary>
    public string Clave { get; set; } = string.Empty;
    public int NumeroEnProyecto { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public int IdProyecto { get; set; }
    public string NombreProyecto { get; set; } = string.Empty;
    public string ClaveProyecto { get; set; } = string.Empty;

    public int IdLista { get; set; }
    public string NombreLista { get; set; } = string.Empty;

    public int? IdTipoActividad { get; set; }
    public string? NombreTipoActividad { get; set; }
    public string? ColorTipoActividad { get; set; }

    public int? IdEstado { get; set; }
    public string? NombreEstado { get; set; }
    public string? ColorEstado { get; set; }

    public int? IdPrioridad { get; set; }
    public string? NombrePrioridad { get; set; }
    public string? ColorPrioridad { get; set; }

    public int? IdResponsable { get; set; }
    public string? NombreResponsable { get; set; }

    public int? IdInformador { get; set; }
    public string? NombreInformador { get; set; }

    public DateTime? FechaVencimiento { get; set; }
    public int Orden { get; set; }

    public DateTime FechaCreacion { get; set; }
    public DateTime FechaModificacion { get; set; }
}
