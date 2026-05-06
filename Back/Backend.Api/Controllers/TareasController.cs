using System.Security.Claims;
using Backend.Application.Common.DTOs;
using Backend.Application.Tareas.Commands;
using Backend.Application.Tareas.DTOs;
using Backend.Application.Tareas.Queries;
using Backend.Application.Tareas.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/tareas")]
[Produces("application/json")]
[Authorize]
public class TareasController : BaseApiController
{
    private readonly ListarTareasUseCase _listar;
    private readonly ObtenerTareaUseCase _obtener;
    private readonly CrearTareaUseCase _crear;
    private readonly ActualizarTareaUseCase _actualizar;
    private readonly EliminarTareaUseCase _eliminar;

    public TareasController(
        ListarTareasUseCase listar,
        ObtenerTareaUseCase obtener,
        CrearTareaUseCase crear,
        ActualizarTareaUseCase actualizar,
        EliminarTareaUseCase eliminar)
    {
        _listar = listar;
        _obtener = obtener;
        _crear = crear;
        _actualizar = actualizar;
        _eliminar = eliminar;
    }

    /// <summary>Lista tareas con filtros (proyecto, lista, responsable, estado, prioridad, q) y paginación.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TareaDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] ListarTareasQuery query, CancellationToken ct)
    {
        var (items, pagination) = await _listar.ExecuteAsync(query, ct);
        return ApiOk(items, pagination);
    }

    /// <summary>Detalle de una tarea.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TareaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Obtener(int id, CancellationToken ct)
        => ApiOk(await _obtener.ExecuteAsync(id, ct));

    /// <summary>Crea una tarea. El usuario actual queda registrado como Informador.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TareaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Crear([FromBody] CrearTareaCommand cmd, CancellationToken ct)
    {
        var idInformador = GetCurrentUserId();
        return ApiOk(await _crear.ExecuteAsync(cmd, idInformador, ct));
    }

    /// <summary>Actualiza una tarea. Permite mover de lista (drag &amp; drop entre columnas Kanban).</summary>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TareaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarTareaCommand cmd, CancellationToken ct)
        => ApiOk(await _actualizar.ExecuteAsync(id, cmd, ct));

    /// <summary>Elimina una tarea.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _eliminar.ExecuteAsync(id, ct);
        return ApiOk(new { eliminado = true });
    }

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
