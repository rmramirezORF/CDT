using Backend.Domain.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations.Catalogos;

public class PrioridadConfiguration : IEntityTypeConfiguration<PrioridadEntity>
{
    public void Configure(EntityTypeBuilder<PrioridadEntity> builder)
    {
        builder.ToTable("Prioridad");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Nombre).IsUnique();
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(60);
        builder.Property(p => p.Color).IsRequired().HasMaxLength(20);
    }
}
