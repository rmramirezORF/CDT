using Backend.Domain.Entities.Proyectos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations.Proyectos;

public class ListaConfiguration : IEntityTypeConfiguration<ListaEntity>
{
    public void Configure(EntityTypeBuilder<ListaEntity> builder)
    {
        builder.ToTable("Lista");

        builder.HasKey(l => l.Id);
        builder.HasIndex(l => l.IdProyecto);
        builder.HasIndex(l => new { l.IdProyecto, l.Orden });

        builder.Property(l => l.Nombre).IsRequired().HasMaxLength(150);
        builder.Property(l => l.Color).HasMaxLength(20);
        builder.Property(l => l.Orden).IsRequired();
        builder.Property(l => l.FechaCreacion).IsRequired();
        builder.Property(l => l.FechaModificacion).IsRequired();

        // Proyecto: Cascade — al borrar un proyecto se borran sus listas.
        builder.HasOne(l => l.Proyecto)
               .WithMany()
               .HasForeignKey(l => l.IdProyecto)
               .OnDelete(DeleteBehavior.Cascade);

        // Creador: SetNull — borrar el usuario deja la lista huérfana.
        builder.HasOne(l => l.Creador)
               .WithMany()
               .HasForeignKey(l => l.IdCreador)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
