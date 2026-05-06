namespace Backend.Application.Auth.DTOs;

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string RolGlobal { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaConfirmacionEmail { get; set; }
}
