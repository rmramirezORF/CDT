using Backend.Domain.Entities.Auth;
using Backend.Domain.Entities.Base;
using Backend.Domain.Entities.Catalogos;
using Backend.Domain.Entities.Proyectos;

namespace Backend.Domain.Entities.Tareas;

public class TareaEntity : AuditableEntity
{
    public int Id { get; set; }

    /// <summary>Número correlativo dentro del proyecto. Combinado con Proyecto.Clave forma el código tipo "ANI-12".</summary>
    public int NumeroEnProyecto { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public int IdProyecto { get; set; }
    public int IdLista { get; set; }
    public int? IdTipoActividad { get; set; }
    public int? IdEstado { get; set; }
    public int? IdPrioridad { get; set; }
    public int? IdResponsable { get; set; }

    /// <summary>Quien creó/reportó la tarea (Jira: "Informador").</summary>
    public int? IdInformador { get; set; }

    public DateTime? FechaVencimiento { get; set; }
    public int Orden { get; set; }

    // Navegación
    public ProyectoEntity Proyecto { get; set; } = null!;
    public ListaEntity Lista { get; set; } = null!;
    public TipoActividadEntity? TipoActividad { get; set; }
    public EstadoEntity? Estado { get; set; }
    public PrioridadEntity? Prioridad { get; set; }
    public UsuarioEntity? Responsable { get; set; }
    public UsuarioEntity? Informador { get; set; }
}
