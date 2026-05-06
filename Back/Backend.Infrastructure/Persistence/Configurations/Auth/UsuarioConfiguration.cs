using Backend.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations.Auth;

public class UsuarioConfiguration : IEntityTypeConfiguration<UsuarioEntity>
{
    public void Configure(EntityTypeBuilder<UsuarioEntity> builder)
    {
        builder.ToTable("Usuario");

        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.Correo).IsUnique();

        builder.Property(u => u.Nombre).IsRequired().HasMaxLength(120);
        builder.Property(u => u.Correo).IsRequired().HasMaxLength(120);
        builder.Property(u => u.ClaveHash).IsRequired().HasMaxLength(255);
        builder.Property(u => u.RolGlobal).IsRequired().HasMaxLength(20);
        builder.Property(u => u.CodigoConfirmacionEmail).HasMaxLength(64);

        builder.Property(u => u.FechaCreacion).IsRequired();
        builder.Property(u => u.FechaModificacion).IsRequired();
    }
}
