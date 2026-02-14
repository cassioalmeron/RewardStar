namespace RewardStar.Core.Models;

public class RewardClaim : EntityBase
{
    public int UserId { get; set; }
    public RewardType RewardType { get; set; }
    public int PointsCost { get; set; }
    public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public User? User { get; set; }
}
