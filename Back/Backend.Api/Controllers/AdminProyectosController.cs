using System.Security.Claims;
using Backend.Application.Common.DTOs;
using Backend.Application.Proyectos.Commands;
using Backend.Application.Proyectos.DTOs;
using Backend.Application.Proyectos.Queries;
using Backend.Application.Proyectos.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/admin/proyectos")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminProyectosController : BaseApiController
{
    private readonly ListarProyectosUseCase _listar;
    private readonly ObtenerProyectoUseCase _obtener;
    private readonly CrearProyectoUseCase _crear;
    private readonly ActualizarProyectoUseCase _actualizar;
    private readonly EliminarProyectoUseCase _eliminar;

    public AdminProyectosController(
        ListarProyectosUseCase listar,
        ObtenerProyectoUseCase obtener,
        CrearProyectoUseCase crear,
        ActualizarProyectoUseCase actualizar,
        EliminarProyectoUseCase eliminar)
    {
        _listar = listar;
        _obtener = obtener;
        _crear = crear;
        _actualizar = actualizar;
        _eliminar = eliminar;
    }

    /// <summary>Lista proyectos con filtros (q, idEquipo) y paginación.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ProyectoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] ListarProyectosQuery query, CancellationToken ct)
    {
        var (items, pagination) = await _listar.ExecuteAsync(query, ct);
        return ApiOk(items, pagination);
    }

    /// <summary>Detalle de un proyecto.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProyectoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Obtener(int id, CancellationToken ct)
        => ApiOk(await _obtener.ExecuteAsync(id, ct));

    /// <summary>Crea un proyecto. El creador queda registrado como el usuario actual.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProyectoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Crear([FromBody] CrearProyectoCommand cmd, CancellationToken ct)
    {
        var idCreador = GetCurrentUserId();
        return ApiOk(await _crear.ExecuteAsync(cmd, idCreador, ct));
    }

    /// <summary>Actualiza un proyecto.</summary>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProyectoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarProyectoCommand cmd, CancellationToken ct)
        => ApiOk(await _actualizar.ExecuteAsync(id, cmd, ct));

    /// <summary>Elimina un proyecto.</summary>
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
