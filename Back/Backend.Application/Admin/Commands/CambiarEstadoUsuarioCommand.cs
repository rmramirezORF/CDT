namespace Backend.Application.Admin.Commands;

public class CambiarEstadoUsuarioCommand
{
    /// <summary>true = activo, false = inactivo (no puede iniciar sesión).</summary>
    public bool Estado { get; set; }
}
