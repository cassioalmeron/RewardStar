using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RewardStar.Core.Models;

namespace RewardStar.Core.Configurations;

internal class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("Activity");

        // Foreign Key Relationship
        builder.HasOne(a => a.User)
            .WithMany(u => u.Activities)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Property Configurations
        builder.Property(a => a.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.Level)
            .IsRequired();

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.Position)
            .IsRequired();

        builder.Property(a => a.Active)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes for better query performance
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => new { a.UserId, a.Position });
    }
}