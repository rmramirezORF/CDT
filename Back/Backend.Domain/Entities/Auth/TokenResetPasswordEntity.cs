namespace Backend.Domain.Entities.Auth;

public class TokenResetPasswordEntity
{
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string Codigo6Digitos { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaExpiracion { get; set; }
    public bool Usado { get; set; }

    public UsuarioEntity Usuario { get; set; } = null!;
}
