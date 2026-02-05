using System.ComponentModel.DataAnnotations;

namespace RewardStar.Api.DTOs;

/// <summary>
/// DTO for creating a new activity.
/// Inherits from DtoBase to support automatic mapping to Activity entity via DtoBaseExtension.
/// Includes automatic ID-to-Entity conversion for UserId -> User navigation property.
/// Uses a regular class (not record) for consistency and to support reflection-based mapping.
/// </summary>
public class CreateActivityDto : DtoBase
{
    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Activity level is required")]
    public int Level { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    public int UserId { get; set; }

    /// <summary>
    /// Recurring day indicators - each bool represents whether the activity repeats on that day.
    /// These properties are directly mapped to the Activity entity's corresponding properties.
    /// </summary>
    public bool Monday { get; set; }
    public bool Tuesday { get; set; }
    public bool Wednesday { get; set; }
    public bool Thursday { get; set; }
    public bool Friday { get; set; }

    public int Position { get; set; } = 0;
    public bool Active { get; set; } = true;
}

/// <summary>
/// DTO for updating an existing activity.
/// Inherits from DtoBase to support automatic mapping via DtoBaseExtension.
/// All properties are optional to allow partial updates.
/// Uses a regular class (not record) for consistency and to support reflection-based mapping.
/// </summary>
public class UpdateActivityDto : DtoBase
{
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public int? Level { get; set; }

    public int? UserId { get; set; }

    public bool? Monday { get; set; }
    public bool? Tuesday { get; set; }
    public bool? Wednesday { get; set; }
    public bool? Thursday { get; set; }
    public bool? Friday { get; set; }

    public int? Position { get; set; }
    public bool? Active { get; set; }
}

/// <summary>
/// DTO for returning activity data in API responses.
/// Inherits from DtoBase to support automatic mapping via EntityBaseExtension.
/// Uses a regular class (not record) to ensure reflection-based property setting works correctly.
/// </summary>
public class ActivityDto : DtoBase
{
    public string? Description { get; set; }
    public int Level { get; set; }
    public int UserId { get; set; }
    public int Position { get; set; }
    public bool Active { get; set; }
    public bool Monday { get; set; }
    public bool Tuesday { get; set; }
    public bool Wednesday { get; set; }
    public bool Thursday { get; set; }
    public bool Friday { get; set; }
}
