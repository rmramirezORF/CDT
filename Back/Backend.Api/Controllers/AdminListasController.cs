using System.Security.Claims;
using Backend.Application.Common.DTOs;
using Backend.Application.Listas.Commands;
using Backend.Application.Listas.DTOs;
using Backend.Application.Listas.Queries;
using Backend.Application.Listas.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/admin/listas")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminListasController : BaseApiController
{
    private readonly ListarListasUseCase _listar;
    private readonly ObtenerListaUseCase _obtener;
    private readonly CrearListaUseCase _crear;
    private readonly ActualizarListaUseCase _actualizar;
    private readonly EliminarListaUseCase _eliminar;
    private readonly ReordenarListasUseCase _reordenar;

    public AdminListasController(
        ListarListasUseCase listar,
        ObtenerListaUseCase obtener,
        CrearListaUseCase crear,
        ActualizarListaUseCase actualizar,
        EliminarListaUseCase eliminar,
        ReordenarListasUseCase reordenar)
    {
        _listar = listar;
        _obtener = obtener;
        _crear = crear;
        _actualizar = actualizar;
        _eliminar = eliminar;
        _reordenar = reordenar;
    }

    /// <summary>Lista las listas de un proyecto, ordenadas por Orden ASC.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ListaDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] ListarListasQuery query, CancellationToken ct)
        => ApiOk(await _listar.ExecuteAsync(query, ct));

    /// <summary>Detalle de una lista.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ListaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Obtener(int id, CancellationToken ct)
        => ApiOk(await _obtener.ExecuteAsync(id, ct));

    /// <summary>Crea una lista. Si no se envía Orden, se posiciona al final.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ListaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Crear([FromBody] CrearListaCommand cmd, CancellationToken ct)
    {
        var idCreador = GetCurrentUserId();
        return ApiOk(await _crear.ExecuteAsync(cmd, idCreador, ct));
    }

    /// <summary>Actualiza nombre y/o color de una lista. El orden se cambia con /reordenar.</summary>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ListaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarListaCommand cmd, CancellationToken ct)
        => ApiOk(await _actualizar.ExecuteAsync(id, cmd, ct));

    /// <summary>Elimina una lista.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _eliminar.ExecuteAsync(id, ct);
        return ApiOk(new { eliminado = true });
    }

    /// <summary>Reordena las listas de un proyecto en lote (drag &amp; drop).</summary>
    [HttpPost("reordenar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Reordenar([FromBody] ReordenarListasCommand cmd, CancellationToken ct)
    {
        await _reordenar.ExecuteAsync(cmd, ct);
        return ApiOk(new { reordenado = true });
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
