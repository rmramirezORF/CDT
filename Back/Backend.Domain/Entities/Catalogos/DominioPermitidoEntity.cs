namespace Backend.Domain.Entities.Catalogos;

/// <summary>
/// Dominio de correo permitido para registro (lista blanca).
/// Validacion autoritativa en RegisterUseCase, gestionable por Admin desde el panel.
/// </summary>
public class DominioPermitidoEntity
{
    public int Id { get; set; }
    public string Dominio { get; set; } = string.Empty;
}
