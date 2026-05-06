namespace Backend.Domain.Entities.Catalogos;

public class EtiquetaEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Color { get; set; } = "#6b7280";
    public int Orden { get; set; }
}
