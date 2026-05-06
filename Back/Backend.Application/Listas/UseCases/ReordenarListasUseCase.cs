using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Application.Listas.Commands;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Listas.UseCases;

public class ReordenarListasUseCase
{
    private readonly ICdtDbContext _context;

    public ReordenarListasUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(ReordenarListasCommand cmd, CancellationToken ct = default)
    {
        var proyectoExiste = await _context.Proyectos.AnyAsync(p => p.Id == cmd.IdProyecto, ct);
        if (!proyectoExiste)
            throw new BusinessException("PROYECTO_NOT_FOUND", "El proyecto no existe.");

        var ids = cmd.Items.Select(i => i.Id).ToList();
        var listas = await _context.Listas
            .Where(l => l.IdProyecto == cmd.IdProyecto && ids.Contains(l.Id))
            .ToListAsync(ct);

        if (listas.Count != ids.Count)
            throw new BusinessException(
                "LISTA_FUERA_DE_PROYECTO",
                "Alguna lista no pertenece al proyecto o no existe.");

        var ordenPorId = cmd.Items.ToDictionary(i => i.Id, i => i.Orden);
        var ahora = DateTime.UtcNow;
        foreach (var lista in listas)
        {
            lista.Orden = ordenPorId[lista.Id];
            lista.FechaModificacion = ahora;
        }

        await _context.SaveChangesAsync(ct);
    }
}
