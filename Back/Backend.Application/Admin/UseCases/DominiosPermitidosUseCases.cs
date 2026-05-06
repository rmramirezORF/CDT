using Backend.Application.Admin.Commands;
using Backend.Application.Admin.DTOs;
using Backend.Application.Common.Exceptions;
using Backend.Application.Common.Persistence;
using Backend.Domain.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Admin.UseCases;

/// <summary>CRUD de dominios permitidos para registro (lista blanca).</summary>
public class DominiosPermitidosUseCases
{
    private readonly ICdtDbContext _context;

    public DominiosPermitidosUseCases(ICdtDbContext context)
    {
        _context = context;
    }

    public async Task<List<DominioPermitidoDto>> ListarAsync(CancellationToken ct = default) =>
        await _context.DominiosPermitidos
            .AsNoTracking()
            .OrderBy(d => d.Dominio)
            .Select(d => new DominioPermitidoDto { Id = d.Id, Dominio = d.Dominio })
            .ToListAsync(ct);

    public async Task<DominioPermitidoDto> CrearAsync(CrearDominioPermitidoCommand cmd, CancellationToken ct = default)
    {
        var dominio = cmd.Dominio.Trim().ToLowerInvariant();
        var entity = new DominioPermitidoEntity { Dominio = dominio };
        _context.DominiosPermitidos.Add(entity);
        try { await _context.SaveChangesAsync(ct); }
        catch (DbUpdateException) { throw new BusinessException("DOMINIO_DUPLICADO", "Ese dominio ya está en la lista."); }
        return new DominioPermitidoDto { Id = entity.Id, Dominio = entity.Dominio };
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.DominiosPermitidos.FirstOrDefaultAsync(d => d.Id == id, ct)
            ?? throw new BusinessException("DOMINIO_NOT_FOUND", "El dominio no existe.");

        // Validar que no quede la lista vacía (sino nadie podría registrarse).
        var totalRestante = await _context.DominiosPermitidos.CountAsync(ct);
        if (totalRestante <= 1)
            throw new BusinessException(
                "DOMINIO_LIST_EMPTY",
                "Debe quedar al menos un dominio permitido. Si dejás la lista vacía, nadie podrá registrarse.");

        _context.DominiosPermitidos.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
