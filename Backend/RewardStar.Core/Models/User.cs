namespace RewardStar.Core.Models;

public class User : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }  // Nullable - not required when GoogleAuthId is present
    public string? GoogleAuthId { get; set; }  // Nullable - only for Google sign-ins
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation Properties
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<GameCompletion> GameCompletions { get; set; } = new List<GameCompletion>();
    public ICollection<RewardClaim> RewardClaims { get; set; } = new List<RewardClaim>();
    public GameState? GameState { get; set; }
}
