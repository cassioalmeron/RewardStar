using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RewardStart.Core.Models;

namespace RewardStart.Core.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.GoogleAuthId)
            .IsUnique()
            .HasFilter("\"GoogleAuthId\" IS NOT NULL");

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(320);  // RFC 5321 max email length

        builder.Property(u => u.Password)
            .HasMaxLength(500);  // Sufficient for bcrypt hashes

        builder.Property(u => u.GoogleAuthId)
            .HasMaxLength(255);

        builder.Property(u => u.Active)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .IsRequired();
    }
}
