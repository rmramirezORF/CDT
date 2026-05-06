using System.Security.Claims;
using Backend.Application.Admin.Commands;
using Backend.Application.Admin.DTOs;
using Backend.Application.Admin.Queries;
using Backend.Application.Admin.UseCases;
using Backend.Application.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/admin/usuarios")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminUsuariosController : BaseApiController
{
    private readonly ListarUsuariosUseCase _listar;
    private readonly CambiarRolUsuarioUseCase _cambiarRol;
    private readonly CambiarEstadoUsuarioUseCase _cambiarEstado;
    private readonly EliminarUsuarioUseCase _eliminar;
    private readonly ConfirmarEmailManualmenteUseCase _confirmarEmail;

    public AdminUsuariosController(
        ListarUsuariosUseCase listar,
        CambiarRolUsuarioUseCase cambiarRol,
        CambiarEstadoUsuarioUseCase cambiarEstado,
        EliminarUsuarioUseCase eliminar,
        ConfirmarEmailManualmenteUseCase confirmarEmail)
    {
        _listar = listar;
        _cambiarRol = cambiarRol;
        _cambiarEstado = cambiarEstado;
        _eliminar = eliminar;
        _confirmarEmail = confirmarEmail;
    }

    /// <summary>Lista usuarios con búsqueda + paginación.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<UsuarioListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] ListarUsuariosQuery query, CancellationToken ct)
    {
        var (items, pagination) = await _listar.ExecuteAsync(query, ct);
        return ApiOk(items, pagination);
    }

    /// <summary>Cambia el rol global de un usuario (Admin / Lider / Miembro).</summary>
    [HttpPatch("{id:int}/rol")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CambiarRol(int id, [FromBody] CambiarRolUsuarioCommand cmd, CancellationToken ct)
        => ApiOk(await _cambiarRol.ExecuteAsync(id, cmd, ct));

    /// <summary>Activa o desactiva un usuario. Al desactivar revoca todos sus refresh tokens.</summary>
    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoUsuarioCommand cmd, CancellationToken ct)
        => ApiOk(await _cambiarEstado.ExecuteAsync(id, cmd, ct));

    /// <summary>
    /// Elimina permanentemente un usuario (cascada: refresh tokens + tokens de reset).
    /// Un Admin no puede eliminar su propia cuenta.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        var idActual = GetCurrentUserId();
        await _eliminar.ExecuteAsync(id, idActual, ct);
        return ApiOk(new { eliminado = true });
    }

    /// <summary>
    /// Marca el email de un usuario como confirmado sin necesidad del código.
    /// Útil cuando el usuario no recibe el correo (problemas de SMTP, dominio, spam, etc.).
    /// Idempotente: si ya estaba confirmado no rompe.
    /// </summary>
    [HttpPost("{id:int}/confirmar-email")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmarEmail(int id, CancellationToken ct)
        => ApiOk(await _confirmarEmail.ExecuteAsync(id, ct));

    // ----- helpers -----

    private int GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue("sub");
        if (!int.TryParse(sub, out var id))
            throw new UnauthorizedAccessException("No se pudo identificar el usuario actual.");
        return id;
    }
}
