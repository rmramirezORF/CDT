namespace Backend.Application.Listas.DTOs;

public class ListaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Color { get; set; }
    public int Orden { get; set; }

    public int IdProyecto { get; set; }
    public string NombreProyecto { get; set; } = string.Empty;

    public int? IdCreador { get; set; }
    public string? NombreCreador { get; set; }

    public DateTime FechaCreacion { get; set; }
    public DateTime FechaModificacion { get; set; }
}
