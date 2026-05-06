using Backend.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations.Auth;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
    {
        builder.ToTable("RefreshToken");

        builder.HasKey(rt => rt.Id);
        builder.HasIndex(rt => rt.Token).IsUnique();
        builder.HasIndex(rt => rt.IdUsuario);

        builder.Property(rt => rt.Token).IsRequired().HasMaxLength(255);
        builder.Property(rt => rt.FechaCreacion).IsRequired();
        builder.Property(rt => rt.FechaExpiracion).IsRequired();

        builder.HasOne(rt => rt.Usuario)
               .WithMany()
               .HasForeignKey(rt => rt.IdUsuario)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
