using System.ComponentModel.DataAnnotations;

namespace RewardStar.Api.DTOs;

/// <summary>
/// Response DTO for GET /api/Game/state - returns full game state
/// </summary>
public class GameStateDto : DtoBase
{
    public Dictionary<int, CompletionDaysDto> Completions { get; set; } = new();
    public int TotalPoints { get; set; }
    public int Balance { get; set; }
}

/// <summary>
/// Represents completion status for each day of the week for a single activity
/// </summary>
public class CompletionDaysDto : DtoBase
{
    public bool Monday { get; set; }
    public bool Tuesday { get; set; }
    public bool Wednesday { get; set; }
    public bool Thursday { get; set; }
    public bool Friday { get; set; }
}

/// <summary>
/// Request DTO for POST /api/Game/toggle
/// </summary>
public class ToggleCompletionRequestDto : DtoBase
{
    [Required(ErrorMessage = "Activity ID is required")]
    public int ActivityId { get; set; }

    [Required(ErrorMessage = "Day is required")]
    public string Day { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for POST /api/Game/toggle
/// </summary>
public class ToggleCompletionResponseDto : DtoBase
{
    public bool Completed { get; set; }
    public int TotalPoints { get; set; }
    public int Balance { get; set; }
}

/// <summary>
/// Request DTO for POST /api/Game/claim
/// </summary>
public class ClaimRewardRequestDto : DtoBase
{
    [Required(ErrorMessage = "Reward type is required")]
    public int RewardType { get; set; }
}

/// <summary>
/// Response DTO for POST /api/Game/claim
/// </summary>
public class ClaimRewardResponseDto : DtoBase
{
    public bool Success { get; set; }
    public int Balance { get; set; }
    public int RewardType { get; set; }
    public int PointsCost { get; set; }
}
