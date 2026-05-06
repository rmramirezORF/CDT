using Backend.Domain.Entities.Auth;
using Backend.Domain.Entities.Base;

namespace Backend.Domain.Entities.Proyectos;

public class ListaEntity : AuditableEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int Orden { get; set; }

    public int IdProyecto { get; set; }
    public int? IdCreador { get; set; }

    // Navegación
    public ProyectoEntity Proyecto { get; set; } = null!;
    public UsuarioEntity? Creador { get; set; }
}
