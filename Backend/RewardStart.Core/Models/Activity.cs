namespace RewardStart.Core.Models;

public class Activity : EntityBase
{
    public string Description { get; set; }
    public Level Level { get; set; }

    public bool Monday { get; set; }
    public bool Tuesday { get; set; }
    public bool Wednesday { get; set; }
    public bool Thursday { get; set; }
    public bool Friday { get; set; }
    public int Position { get; set; }
    public bool Active { get; set; }
}

public enum Level
{
    Easy = 1,
    Medium = 2,
    Hard = 3
}