using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RewardStar.Core.Models;

namespace RewardStar.Core.Configurations;

internal class GameCompletionConfiguration : IEntityTypeConfiguration<GameCompletion>
{
    public void Configure(EntityTypeBuilder<GameCompletion> builder)
    {
        builder.ToTable("GameCompletion");

        // Foreign Key Relationships
        builder.HasOne(gc => gc.Activity)
            .WithMany()
            .HasForeignKey(gc => gc.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gc => gc.User)
            .WithMany(u => u.GameCompletions)
            .HasForeignKey(gc => gc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Property Configurations
        builder.Property(gc => gc.ActivityId)
            .IsRequired();

        builder.Property(gc => gc.Day)
            .IsRequired();

        builder.Property(gc => gc.Completed)
            .IsRequired();

        builder.Property(gc => gc.UserId)
            .IsRequired();

        // Unique constraint: one record per activity per day per user
        builder.HasIndex(gc => new { gc.ActivityId, gc.Day, gc.UserId })
            .IsUnique();

        // Index for querying by user
        builder.HasIndex(gc => gc.UserId);
    }
}
