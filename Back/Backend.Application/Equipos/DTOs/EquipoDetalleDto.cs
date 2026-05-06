namespace Backend.Application.Equipos.DTOs;

public class EquipoDetalleDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public int? IdEquipoPadre { get; set; }
    public string? NombreEquipoPadre { get; set; }

    public int? IdLider { get; set; }
    public string? NombreLider { get; set; }
    public string? CorreoLider { get; set; }

    public DateTime FechaCreacion { get; set; }

    public List<MiembroDto> Miembros { get; set; } = new();
    public List<EquipoListItemDto> SubEquipos { get; set; } = new();
}
