using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Application.Listas.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Listas.UseCases;

public class ObtenerListaUseCase
{
    private readonly ICdtDbContext _context;

    public ObtenerListaUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<ListaDto> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var lista = await _context.Listas
            .AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => new ListaDto
            {
                Id = l.Id,
                Nombre = l.Nombre,
                Color = l.Color,
                Orden = l.Orden,
                IdProyecto = l.IdProyecto,
                NombreProyecto = l.Proyecto.Nombre,
                IdCreador = l.IdCreador,
                NombreCreador = l.Creador != null ? l.Creador.Nombre : null,
                FechaCreacion = l.FechaCreacion,
                FechaModificacion = l.FechaModificacion,
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new BusinessException("LISTA_NOT_FOUND", "La lista no existe.");

        return lista;
    }
}
