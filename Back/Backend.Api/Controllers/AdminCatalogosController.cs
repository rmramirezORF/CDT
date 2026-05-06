using Backend.Application.Admin.Commands;
using Backend.Application.Admin.DTOs;
using Backend.Application.Admin.UseCases;
using Backend.Application.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/admin/catalogos")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminCatalogosController : BaseApiController
{
    private readonly EstadosCatalogoUseCases _estados;
    private readonly PrioridadesCatalogoUseCases _prioridades;
    private readonly EtiquetasCatalogoUseCases _etiquetas;
    private readonly TiposActividadCatalogoUseCases _tipos;
    private readonly DominiosPermitidosUseCases _dominios;

    public AdminCatalogosController(
        EstadosCatalogoUseCases estados,
        PrioridadesCatalogoUseCases prioridades,
        EtiquetasCatalogoUseCases etiquetas,
        TiposActividadCatalogoUseCases tipos,
        DominiosPermitidosUseCases dominios)
    {
        _estados = estados;
        _prioridades = prioridades;
        _etiquetas = etiquetas;
        _tipos = tipos;
        _dominios = dominios;
    }

    // ----- Estados -----
    [HttpGet("estados")]
    [ProducesResponseType(typeof(ApiResponse<List<CatalogoItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarEstados(CancellationToken ct)
        => ApiOk(await _estados.ListarAsync(ct));

    [HttpPost("estados")]
    [ProducesResponseType(typeof(ApiResponse<CatalogoItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CrearEstado([FromBody] CrearCatalogoItemCommand cmd, CancellationToken ct)
        => ApiOk(await _estados.CrearAsync(cmd, ct));

    [HttpPatch("estados/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CatalogoItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarCatalogoItemCommand cmd, CancellationToken ct)
        => ApiOk(await _estados.ActualizarAsync(id, cmd, ct));

    [HttpDelete("estados/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EliminarEstado(int id, CancellationToken ct)
    {
        await _estados.EliminarAsync(id, ct);
        return ApiOk(new { eliminado = true });
    }

    // ----- Prioridades -----
    [HttpGet("prioridades")]
    [ProducesResponseType(typeof(ApiResponse<List<CatalogoItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPrioridades(CancellationToken ct)
        => ApiOk(await _prioridades.ListarAsync(ct));

    [HttpPost("prioridades")]
    [ProducesResponseType(typeof(ApiResponse<CatalogoItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CrearPrioridad([FromBody] CrearCatalogoItemCommand cmd, CancellationToken ct)
        => ApiOk(await _prioridades.CrearAsync(cmd, ct));

    [HttpPatch("prioridades/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CatalogoItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActualizarPrioridad(int id, [FromBody] ActualizarCatalogoItemCommand cmd, CancellationToken ct)
        => ApiOk(await _prioridades.ActualizarAsync(id, cmd, ct));

    [HttpDelete("prioridades/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EliminarPrioridad(int id, CancellationToken ct)
    {
        await _prioridades.EliminarAsync(id, ct);
        return ApiOk(new { eliminado = true });
    }

    // ----- Etiquetas -----
    [HttpGet("etiquetas")]
    [ProducesResponseType(typeof(ApiResponse<List<CatalogoItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarEtiquetas(CancellationToken ct)
        => ApiOk(await _etiquetas.ListarAsync(ct));

    [HttpPost("etiquetas")]
    [ProducesResponseType(typeof(ApiResponse<CatalogoItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CrearEtiqueta([FromBody] CrearCatalogoItemCommand cmd, CancellationToken ct)
        => ApiOk(await _etiquetas.CrearAsync(cmd, ct));

    [HttpPatch("etiquetas/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CatalogoItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActualizarEtiqueta(int id, [FromBody] ActualizarCatalogoItemCommand cmd, CancellationToken ct)
        => ApiOk(await _etiquetas.ActualizarAsync(id, cmd, ct));

    [HttpDelete("etiquetas/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EliminarEtiqueta(int id, CancellationToken ct)
    {
        await _etiquetas.EliminarAsync(id, ct);
        return ApiOk(new { eliminado = true });
    }

    // ----- Tipos de Actividad -----
    [HttpGet("tipos-actividad")]
    [ProducesResponseType(typeof(ApiResponse<List<CatalogoItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTiposActividad(CancellationToken ct)
        => ApiOk(await _tipos.ListarAsync(ct));

    [HttpPost("tipos-actividad")]
    [ProducesResponseType(typeof(ApiResponse<CatalogoItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CrearTipoActividad([FromBody] CrearCatalogoItemCommand cmd, CancellationToken ct)
        => ApiOk(await _tipos.CrearAsync(cmd, ct));

    [HttpPatch("tipos-actividad/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CatalogoItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActualizarTipoActividad(int id, [FromBody] ActualizarCatalogoItemCommand cmd, CancellationToken ct)
        => ApiOk(await _tipos.ActualizarAsync(id, cmd, ct));

    [HttpDelete("tipos-actividad/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EliminarTipoActividad(int id, CancellationToken ct)
    {
        await _tipos.EliminarAsync(id, ct);
        return ApiOk(new { eliminado = true });
    }

    // ----- Dominios permitidos para registro -----
    [HttpGet("dominios")]
    [ProducesResponseType(typeof(ApiResponse<List<DominioPermitidoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarDominios(CancellationToken ct)
        => ApiOk(await _dominios.ListarAsync(ct));

    [HttpPost("dominios")]
    [ProducesResponseType(typeof(ApiResponse<DominioPermitidoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CrearDominio([FromBody] CrearDominioPermitidoCommand cmd, CancellationToken ct)
        => ApiOk(await _dominios.CrearAsync(cmd, ct));

    [HttpDelete("dominios/{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EliminarDominio(int id, CancellationToken ct)
    {
        await _dominios.EliminarAsync(id, ct);
        return ApiOk(new { eliminado = true });
    }
}
