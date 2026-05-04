namespace Backend.Application.Common.Persistence;

/// <summary>
/// Contrato del DbContext expuesto a la capa de Application.
/// Los DbSets se agregan a medida que se crean las entidades del dominio.
/// </summary>
public interface ICdtDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
