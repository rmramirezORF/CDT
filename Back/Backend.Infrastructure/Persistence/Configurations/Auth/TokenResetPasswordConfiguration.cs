using Backend.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Infrastructure.Persistence.Configurations.Auth;

public class TokenResetPasswordConfiguration : IEntityTypeConfiguration<TokenResetPasswordEntity>
{
    public void Configure(EntityTypeBuilder<TokenResetPasswordEntity> builder)
    {
        builder.ToTable("TokenResetPassword");

        builder.HasKey(t => t.Id);
        builder.HasIndex(t => new { t.IdUsuario, t.Usado });

        builder.Property(t => t.Codigo6Digitos).IsRequired().HasMaxLength(6);
        builder.Property(t => t.FechaCreacion).IsRequired();
        builder.Property(t => t.FechaExpiracion).IsRequired();

        builder.HasOne(t => t.Usuario)
               .WithMany()
               .HasForeignKey(t => t.IdUsuario)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
