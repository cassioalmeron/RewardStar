using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RewardStart.Core.Models;

namespace RewardStart.Core.Configurations;

internal class RewardClaimConfiguration : IEntityTypeConfiguration<RewardClaim>
{
    public void Configure(EntityTypeBuilder<RewardClaim> builder)
    {
        builder.ToTable("RewardClaim");

        // Foreign Key Relationship
        builder.HasOne(rc => rc.User)
            .WithMany(u => u.RewardClaims)
            .HasForeignKey(rc => rc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Property Configurations
        builder.Property(rc => rc.UserId)
            .IsRequired();

        builder.Property(rc => rc.RewardType)
            .IsRequired();

        builder.Property(rc => rc.PointsCost)
            .IsRequired();

        builder.Property(rc => rc.ClaimedAt)
            .IsRequired();

        // Index for querying by user
        builder.HasIndex(rc => rc.UserId);
    }
}
