using System.ComponentModel.DataAnnotations;

namespace Backend.Application.Admin.Commands;

public class CrearDominioPermitidoCommand
{
    /// <summary>
    /// Dominio sin la arroba ni el http://. Ej: "orf.com.co", "tdh.com.co", "cliente.com".
    /// </summary>
    [Required(ErrorMessage = "El dominio es obligatorio.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "El dominio debe tener entre 3 y 120 caracteres.")]
    [RegularExpression(
        @"^[a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?)+$",
        ErrorMessage = "Dominio con formato inválido. Ej: empresa.com o sub.empresa.com.co")]
    public string Dominio { get; set; } = string.Empty;
}
