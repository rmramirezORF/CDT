using Backend.Domain.Entities.Tareas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations.Tareas;

public class TareaConfiguration : IEntityTypeConfiguration<TareaEntity>
{
    public void Configure(EntityTypeBuilder<TareaEntity> builder)
    {
        builder.ToTable("Tarea");

        builder.HasKey(t => t.Id);
        builder.HasIndex(t => t.IdProyecto);
        builder.HasIndex(t => t.IdLista);
        builder.HasIndex(t => new { t.IdLista, t.Orden });
        builder.HasIndex(t => new { t.IdProyecto, t.NumeroEnProyecto }).IsUnique();
        builder.HasIndex(t => t.IdResponsable);

        builder.Property(t => t.Titulo).IsRequired().HasMaxLength(300);
        builder.Property(t => t.Descripcion).HasMaxLength(4000);
        builder.Property(t => t.NumeroEnProyecto).IsRequired();
        builder.Property(t => t.Orden).IsRequired();
        builder.Property(t => t.FechaCreacion).IsRequired();
        builder.Property(t => t.FechaModificacion).IsRequired();

        // Proyecto: Cascade — al borrar un proyecto se borran sus tareas (ya cascadea Lista, mantenemos consistencia).
        builder.HasOne(t => t.Proyecto)
               .WithMany()
               .HasForeignKey(t => t.IdProyecto)
               .OnDelete(DeleteBehavior.Restrict);

        // Lista: Cascade — al borrar una lista se borran sus tareas. Sólo UN cascade path desde proyecto.
        builder.HasOne(t => t.Lista)
               .WithMany()
               .HasForeignKey(t => t.IdLista)
               .OnDelete(DeleteBehavior.Cascade);

        // Catálogos: SetNull (si se borra el catálogo, la tarea queda sin tipo/estado/prioridad).
        builder.HasOne(t => t.TipoActividad)
               .WithMany()
               .HasForeignKey(t => t.IdTipoActividad)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Estado)
               .WithMany()
               .HasForeignKey(t => t.IdEstado)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Prioridad)
               .WithMany()
               .HasForeignKey(t => t.IdPrioridad)
               .OnDelete(DeleteBehavior.SetNull);

        // Usuarios: NoAction para evitar cascade paths múltiples (SQL Server).
        builder.HasOne(t => t.Responsable)
               .WithMany()
               .HasForeignKey(t => t.IdResponsable)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(t => t.Informador)
               .WithMany()
               .HasForeignKey(t => t.IdInformador)
               .OnDelete(DeleteBehavior.NoAction);
    }
}
