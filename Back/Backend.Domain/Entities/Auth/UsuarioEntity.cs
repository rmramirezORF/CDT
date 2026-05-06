using Backend.Domain.Entities.Base;

namespace Backend.Domain.Entities.Auth;

public class UsuarioEntity : AuditableEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string ClaveHash { get; set; } = string.Empty;
    public bool Estado { get; set; } = true;
    public DateTime? FechaConfirmacionEmail { get; set; }
    public string RolGlobal { get; set; } = "Miembro";

    // Confirmación de email pendiente
    public string? CodigoConfirmacionEmail { get; set; }
    public DateTime? FechaExpiracionConfirmacion { get; set; }
}
