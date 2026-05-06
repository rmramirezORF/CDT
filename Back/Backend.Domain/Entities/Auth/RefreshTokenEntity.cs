namespace Backend.Domain.Entities.Auth;

public class RefreshTokenEntity
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaExpiracion { get; set; }
    public bool Revocado { get; set; }

    public UsuarioEntity Usuario { get; set; } = null!;
}
