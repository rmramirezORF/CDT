namespace Backend.Application.Admin.Queries;

public class ListarUsuariosQuery
{
    /// <summary>Búsqueda en nombre o correo.</summary>
    public string? Q { get; set; }

    /// <summary>Filtrar por rol (Admin / Lider / Miembro).</summary>
    public string? Rol { get; set; }

    /// <summary>Filtrar por estado activo/inactivo.</summary>
    public bool? Estado { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
