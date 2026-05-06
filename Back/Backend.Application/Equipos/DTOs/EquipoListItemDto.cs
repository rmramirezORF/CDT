namespace Backend.Application.Equipos.DTOs;

public class EquipoListItemDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public int? IdEquipoPadre { get; set; }
    public string? NombreEquipoPadre { get; set; }

    public int? IdLider { get; set; }
    public string? NombreLider { get; set; }
    public string? CorreoLider { get; set; }

    public int TotalMiembros { get; set; }
    public int TotalSubEquipos { get; set; }

    public DateTime FechaCreacion { get; set; }
}
