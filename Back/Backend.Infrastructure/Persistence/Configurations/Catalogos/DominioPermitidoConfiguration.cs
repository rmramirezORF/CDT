using Backend.Domain.Entities.Catalogos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations.Catalogos;

public class DominioPermitidoConfiguration : IEntityTypeConfiguration<DominioPermitidoEntity>
{
    public void Configure(EntityTypeBuilder<DominioPermitidoEntity> builder)
    {
        builder.ToTable("DominioPermitido");
        builder.HasKey(d => d.Id);
        builder.HasIndex(d => d.Dominio).IsUnique();
        builder.Property(d => d.Dominio).IsRequired().HasMaxLength(120);
    }
}
