using System.Reflection;
using Backend.Application.Common.Persistence;
using Backend.Domain.Entities.Auth;
using Backend.Domain.Entities.Catalogos;
using Backend.Domain.Entities.Equipos;
using Backend.Domain.Entities.Proyectos;
using Backend.Domain.Entities.Tareas;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Common.Persistence;

public class CdtDbContext : DbContext, ICdtDbContext
{
    public CdtDbContext(DbContextOptions<CdtDbContext> options) : base(options) { }

    public DbSet<UsuarioEntity> Usuarios => Set<UsuarioEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    public DbSet<TokenResetPasswordEntity> TokensResetPassword => Set<TokenResetPasswordEntity>();

    public DbSet<EstadoEntity> Estados => Set<EstadoEntity>();
    public DbSet<PrioridadEntity> Prioridades => Set<PrioridadEntity>();
    public DbSet<EtiquetaEntity> Etiquetas => Set<EtiquetaEntity>();
    public DbSet<TipoActividadEntity> TiposActividad => Set<TipoActividadEntity>();
    public DbSet<DominioPermitidoEntity> DominiosPermitidos => Set<DominioPermitidoEntity>();

    public DbSet<EquipoEntity> Equipos => Set<EquipoEntity>();
    public DbSet<EquipoMiembroEntity> EquiposMiembros => Set<EquipoMiembroEntity>();

    public DbSet<ProyectoEntity> Proyectos => Set<ProyectoEntity>();
    public DbSet<ListaEntity> Listas => Set<ListaEntity>();

    public DbSet<TareaEntity> Tareas => Set<TareaEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
