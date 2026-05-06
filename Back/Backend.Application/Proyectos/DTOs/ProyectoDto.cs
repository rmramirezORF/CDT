namespace Backend.Application.Proyectos.DTOs;

public class ProyectoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public int IdEquipo { get; set; }
    public string NombreEquipo { get; set; } = string.Empty;

    public int? IdCreador { get; set; }
    public string? NombreCreador { get; set; }

    public DateTime FechaCreacion { get; set; }
    public DateTime FechaModificacion { get; set; }
}
