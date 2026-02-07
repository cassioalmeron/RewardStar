namespace RewardStart.Core.Models;

public class GameState : EntityBase
{
    public int UserId { get; set; }
    public int TotalPoints { get; set; }
    public int Balance { get; set; }

    // Navigation Property
    public User? User { get; set; }
}
