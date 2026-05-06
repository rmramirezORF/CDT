using Backend.Domain.Entities.Auth;

namespace Backend.Domain.Entities.Equipos;

/// <summary>Tabla M:N entre Equipo y Usuario.</summary>
public class EquipoMiembroEntity
{
    public int IdEquipo { get; set; }
    public int IdUsuario { get; set; }
    public DateTime FechaAgregado { get; set; } = DateTime.UtcNow;

    public EquipoEntity Equipo { get; set; } = null!;
    public UsuarioEntity Usuario { get; set; } = null!;
}
