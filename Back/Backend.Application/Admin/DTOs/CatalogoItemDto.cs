namespace Backend.Application.Admin.DTOs;

/// <summary>
/// DTO compartido para los 3 catálogos parametrizables (Estados, Prioridades, Etiquetas)
/// — todos tienen la misma forma.
/// </summary>
public class CatalogoItemDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Orden { get; set; }
}
