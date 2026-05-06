using Backend.Domain.Entities.Auth;
using Backend.Domain.Entities.Base;

namespace Backend.Domain.Entities.Equipos;

public class EquipoEntity : AuditableEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    // Jerarquía: nullable. Equipos raíz tienen IdEquipoPadre = null.
    public int? IdEquipoPadre { get; set; }

    // Líder: nullable para que la eliminación de un usuario no rompa equipos (SetNull).
    public int? IdLider { get; set; }

    // Navegación
    public EquipoEntity? EquipoPadre { get; set; }
    public UsuarioEntity? Lider { get; set; }
    public ICollection<EquipoEntity> SubEquipos { get; set; } = new List<EquipoEntity>();
    public ICollection<EquipoMiembroEntity> Miembros { get; set; } = new List<EquipoMiembroEntity>();
}
