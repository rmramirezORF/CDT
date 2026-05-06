using Backend.Domain.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations.Catalogos;

public class TipoActividadConfiguration : IEntityTypeConfiguration<TipoActividadEntity>
{
    public void Configure(EntityTypeBuilder<TipoActividadEntity> builder)
    {
        builder.ToTable("TipoActividad");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Nombre).IsUnique();
        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(60);
        builder.Property(e => e.Color).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Icono).HasMaxLength(50);
    }
}
