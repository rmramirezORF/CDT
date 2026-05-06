using Backend.Domain.Entities.Equipos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations.Equipos;

public class EquipoMiembroConfiguration : IEntityTypeConfiguration<EquipoMiembroEntity>
{
    public void Configure(EntityTypeBuilder<EquipoMiembroEntity> builder)
    {
        builder.ToTable("EquipoMiembro");

        // Composite key
        builder.HasKey(em => new { em.IdEquipo, em.IdUsuario });
        builder.HasIndex(em => em.IdUsuario);

        builder.Property(em => em.FechaAgregado).IsRequired();

        // Cascade al eliminar el equipo (los miembros se van con el).
        builder.HasOne(em => em.Equipo)
               .WithMany(e => e.Miembros)
               .HasForeignKey(em => em.IdEquipo)
               .OnDelete(DeleteBehavior.Cascade);

        // NoAction al eliminar usuario — requiere limpieza explícita en EliminarUsuarioUseCase.
        // (Si fuera Cascade, SQL Server detecta multi-path y rechaza la migración.)
        builder.HasOne(em => em.Usuario)
               .WithMany()
               .HasForeignKey(em => em.IdUsuario)
               .OnDelete(DeleteBehavior.NoAction);
    }
}
