namespace Backend.Application.Equipos.DTOs;

public class MiembroDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string RolGlobal { get; set; } = string.Empty;
    public DateTime FechaAgregado { get; set; }
}
