namespace Backend.Domain.Entities.Catalogos;

public class TipoActividadEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Color { get; set; } = "#6b7280";
    public int Orden { get; set; }
    /// <summary>Icono lucide opcional (ej: "circle-check", "bug", "lightbulb").</summary>
    public string? Icono { get; set; }
}
