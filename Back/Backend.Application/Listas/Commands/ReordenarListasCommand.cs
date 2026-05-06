using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Listas.Commands;

public class ReordenarListasCommand
{
    [Required(ErrorMessage = "El proyecto es obligatorio.")]
    public int IdProyecto { get; set; }

    /// <summary>
    /// Lista de pares { idLista, orden } con el nuevo orden deseado.
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "Debe enviar al menos una lista.")]
    public List<ItemReorden> Items { get; set; } = new();
}

public class ItemReorden
{
    public int Id { get; set; }
    public int Orden { get; set; }
}
