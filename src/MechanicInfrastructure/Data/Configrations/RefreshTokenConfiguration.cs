using MechanicDomain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicInfrastructure.Data.Configrations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);
        builder.ToTable($"{nameof(RefreshToken)}s");


        builder.Property(rt => rt.Token).IsRequired().HasMaxLength(200);

        builder.HasIndex(rt => rt.Token).IsUnique();

        builder.Property(rt => rt.ExpiresOnUtc).IsRequired();
        builder.Property(rt => rt.UserId).IsRequired();

    }
}
