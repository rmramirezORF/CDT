using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Listas.UseCases;

public class EliminarListaUseCase
{
    private readonly ICdtDbContext _context;

    public EliminarListaUseCase(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task ExecuteAsync(int id, CancellationToken ct = default)
    {
        var lista = await _context.Listas.FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new BusinessException("LISTA_NOT_FOUND", "La lista no existe.");

        // V2: si la lista tiene tareas, decidir si bloquear o cascadear.
        // Por ahora hard delete (entity todavía no tiene Tareas como hijos).
        _context.Listas.Remove(lista);
        await _context.SaveChangesAsync(ct);
    }
}
