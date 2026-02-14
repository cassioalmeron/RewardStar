using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RewardStar.Core.Models;

namespace RewardStar.Core.Configurations;

internal class GameStateConfiguration : IEntityTypeConfiguration<GameState>
{
    public void Configure(EntityTypeBuilder<GameState> builder)
    {
        builder.ToTable("GameState");

        // One-to-one relationship with User
        builder.HasOne(gs => gs.User)
            .WithOne(u => u.GameState)
            .HasForeignKey<GameState>(gs => gs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Property Configurations
        builder.Property(gs => gs.UserId)
            .IsRequired();

        builder.Property(gs => gs.TotalPoints)
            .IsRequired();

        builder.Property(gs => gs.Balance)
            .IsRequired();

        // Unique constraint: one GameState per user
        builder.HasIndex(gs => gs.UserId)
            .IsUnique();
    }
}
