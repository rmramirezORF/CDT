using Backend.Domain.Entities.Equipos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations.Equipos;

public class EquipoConfiguration : IEntityTypeConfiguration<EquipoEntity>
{
    public void Configure(EntityTypeBuilder<EquipoEntity> builder)
    {
        builder.ToTable("Equipo");

        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Nombre);
        builder.HasIndex(e => e.IdEquipoPadre);

        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(120);
        builder.Property(e => e.FechaCreacion).IsRequired();
        builder.Property(e => e.FechaModificacion).IsRequired();

        // Jerarquía padre/hijo (auto-referencia). Restrict: no se puede borrar un equipo que tiene hijos.
        builder.HasOne(e => e.EquipoPadre)
               .WithMany(e => e.SubEquipos)
               .HasForeignKey(e => e.IdEquipoPadre)
               .OnDelete(DeleteBehavior.Restrict);

        // Líder: SetNull al eliminar el usuario, así el equipo no se rompe.
        builder.HasOne(e => e.Lider)
               .WithMany()
               .HasForeignKey(e => e.IdLider)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
