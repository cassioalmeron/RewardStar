namespace RewardStart.Core.Models;

public class User : EntityBase
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Role { get; set; }
} 