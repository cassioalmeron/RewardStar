namespace RewardStart.Core.Models;

public class GameCompletion : EntityBase
{
    public int ActivityId { get; set; }
    public GameDayOfWeek Day { get; set; }
    public bool Completed { get; set; }
    public int UserId { get; set; }

    // Navigation Properties
    public Activity? Activity { get; set; }
    public User? User { get; set; }
}
