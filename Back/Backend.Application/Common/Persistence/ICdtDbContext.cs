using Backend.Domain.Entities.Auth;
using Backend.Domain.Entities.Catalogos;
using Backend.Domain.Entities.Equipos;
using Backend.Domain.Entities.Proyectos;
using Backend.Domain.Entities.Tareas;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Common.Persistence;

/// <summary>
/// Contrato del DbContext expuesto a la capa de Application.
/// Los DbSets se agregan a medida que se crean las entidades del dominio.
/// </summary>
public interface ICdtDbContext
{
    // Auth
    DbSet<UsuarioEntity> Usuarios { get; }
    DbSet<RefreshTokenEntity> RefreshTokens { get; }
    DbSet<TokenResetPasswordEntity> TokensResetPassword { get; }

    // Catalogos parametrizables
    DbSet<EstadoEntity> Estados { get; }
    DbSet<PrioridadEntity> Prioridades { get; }
    DbSet<EtiquetaEntity> Etiquetas { get; }
    DbSet<TipoActividadEntity> TiposActividad { get; }
    DbSet<DominioPermitidoEntity> DominiosPermitidos { get; }

    // Equipos
    DbSet<EquipoEntity> Equipos { get; }
    DbSet<EquipoMiembroEntity> EquiposMiembros { get; }

    // Proyectos
    DbSet<ProyectoEntity> Proyectos { get; }
    DbSet<ListaEntity> Listas { get; }

    // Tareas
    DbSet<TareaEntity> Tareas { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
