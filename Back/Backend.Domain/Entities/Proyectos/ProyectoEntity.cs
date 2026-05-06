using Backend.Domain.Entities.Auth;
using Backend.Domain.Entities.Base;
using Backend.Domain.Entities.Equipos;

namespace Backend.Domain.Entities.Proyectos;

public class ProyectoEntity : AuditableEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    /// <summary>
    /// Clave corta y única tipo Jira (ej: "AN", "TAR", "DEV").
    /// Las tareas se identifican como {Clave}-{NumeroEnProyecto}.
    /// </summary>
    public string Clave { get; set; } = string.Empty;

    /// <summary>
    /// Contador autoincremental de tareas dentro del proyecto.
    /// La próxima tarea creada usará UltimoNumeroTarea + 1.
    /// </summary>
    public int UltimoNumeroTarea { get; set; }

    public int IdEquipo { get; set; }
    public int? IdCreador { get; set; }

    // Navegación
    public EquipoEntity Equipo { get; set; } = null!;
    public UsuarioEntity? Creador { get; set; }
}
