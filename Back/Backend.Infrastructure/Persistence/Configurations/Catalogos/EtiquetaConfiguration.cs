using Backend.Domain.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations.Catalogos;

public class EtiquetaConfiguration : IEntityTypeConfiguration<EtiquetaEntity>
{
    public void Configure(EntityTypeBuilder<EtiquetaEntity> builder)
    {
        builder.ToTable("Etiqueta");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Nombre).IsUnique();
        builder.Property(e => e.Nombre).IsRequired().HasMaxLength(60);
        builder.Property(e => e.Color).IsRequired().HasMaxLength(20);
    }
}
