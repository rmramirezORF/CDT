namespace Backend.Domain.Entities.Base;

public abstract class AuditableEntity
{
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;
    public int IdUsuarioCreacion { get; set; }
    public int IdUsuarioModificacion { get; set; }
}
