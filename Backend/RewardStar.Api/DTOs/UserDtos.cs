using System.ComponentModel.DataAnnotations;

namespace RewardStar.Api.DTOs;

/// <summary>
/// DTO for creating a new user.
/// Inherits from DtoBase to support automatic mapping to User entity via DtoBaseExtension.
/// Uses a regular class (not record) for consistency and to support reflection-based mapping.
/// </summary>
public class CreateUserDto : DtoBase
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(320, ErrorMessage = "Email cannot exceed 320 characters")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    public string? Password { get; set; }
}

/// <summary>
/// DTO for updating an existing user.
/// Inherits from DtoBase to support automatic mapping via DtoBaseExtension.
/// Uses a regular class (not record) for consistency and to support reflection-based mapping.
/// </summary>
public class UpdateUserDto : DtoBase
{
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string? Name { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(320, ErrorMessage = "Email cannot exceed 320 characters")]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    public string? Password { get; set; }
}
