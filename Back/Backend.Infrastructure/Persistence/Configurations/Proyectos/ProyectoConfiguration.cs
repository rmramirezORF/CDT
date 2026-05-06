using Backend.Domain.Entities.Proyectos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations.Proyectos;

public class ProyectoConfiguration : IEntityTypeConfiguration<ProyectoEntity>
{
    public void Configure(EntityTypeBuilder<ProyectoEntity> builder)
    {
        builder.ToTable("Proyecto");

        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.IdEquipo);
        builder.HasIndex(p => p.Nombre);

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Descripcion).HasMaxLength(2000);
        builder.Property(p => p.Clave).IsRequired().HasMaxLength(10);
        builder.Property(p => p.UltimoNumeroTarea).IsRequired().HasDefaultValue(0);
        builder.Property(p => p.FechaCreacion).IsRequired();
        builder.Property(p => p.FechaModificacion).IsRequired();

        builder.HasIndex(p => p.Clave).IsUnique();

        // Equipo: Restrict — no se puede borrar un equipo que tiene proyectos.
        builder.HasOne(p => p.Equipo)
               .WithMany()
               .HasForeignKey(p => p.IdEquipo)
               .OnDelete(DeleteBehavior.Restrict);

        // Creador: SetNull — borrar el usuario deja el proyecto huérfano (no lo elimina).
        builder.HasOne(p => p.Creador)
               .WithMany()
               .HasForeignKey(p => p.IdCreador)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
