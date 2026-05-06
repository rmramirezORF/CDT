using Backend.Application.Common.DTOs;
using Backend.Application.Equipos.Commands;
using Backend.Application.Equipos.DTOs;
using Backend.Application.Equipos.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/admin/equipos")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminEquiposController : BaseApiController
{
    private readonly ListarEquiposUseCase _listar;
    private readonly ObtenerEquipoUseCase _obtener;
    private readonly CrearEquipoUseCase _crear;
    private readonly ActualizarEquipoUseCase _actualizar;
    private readonly EliminarEquipoUseCase _eliminar;
    private readonly AgregarMiembroUseCase _agregarMiembro;
    private readonly QuitarMiembroUseCase _quitarMiembro;

    public AdminEquiposController(
        ListarEquiposUseCase listar,
        ObtenerEquipoUseCase obtener,
        CrearEquipoUseCase crear,
        ActualizarEquipoUseCase actualizar,
        EliminarEquipoUseCase eliminar,
        AgregarMiembroUseCase agregarMiembro,
        QuitarMiembroUseCase quitarMiembro)
    {
        _listar = listar;
        _obtener = obtener;
        _crear = crear;
        _actualizar = actualizar;
        _eliminar = eliminar;
        _agregarMiembro = agregarMiembro;
        _quitarMiembro = quitarMiembro;
    }

    /// <summary>Lista todos los equipos (vista plana, frontend arma la jerarquía).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<EquipoListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
        => ApiOk(await _listar.ExecuteAsync(ct));

    /// <summary>Detalle de un equipo con sus miembros y sub-equipos directos.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EquipoDetalleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Obtener(int id, CancellationToken ct)
        => ApiOk(await _obtener.ExecuteAsync(id, ct));

    /// <summary>Crea un equipo nuevo.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EquipoListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Crear([FromBody] CrearEquipoCommand cmd, CancellationToken ct)
        => ApiOk(await _crear.ExecuteAsync(cmd, ct));

    /// <summary>Actualiza un equipo. Valida no-ciclos en la jerarquía.</summary>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EquipoListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarEquipoCommand cmd, CancellationToken ct)
        => ApiOk(await _actualizar.ExecuteAsync(id, cmd, ct));

    /// <summary>Elimina un equipo. Bloqueado si tiene sub-equipos o miembros.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _eliminar.ExecuteAsync(id, ct);
        return ApiOk(new { eliminado = true });
    }

    /// <summary>Agrega un usuario como miembro del equipo.</summary>
    [HttpPost("{id:int}/miembros")]
    [ProducesResponseType(typeof(ApiResponse<MiembroDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AgregarMiembro(int id, [FromBody] AgregarMiembroCommand cmd, CancellationToken ct)
        => ApiOk(await _agregarMiembro.ExecuteAsync(id, cmd, ct));

    /// <summary>Quita un usuario del equipo.</summary>
    [HttpDelete("{id:int}/miembros/{idUsuario:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> QuitarMiembro(int id, int idUsuario, CancellationToken ct)
    {
        await _quitarMiembro.ExecuteAsync(id, idUsuario, ct);
        return ApiOk(new { eliminado = true });
    }
}
