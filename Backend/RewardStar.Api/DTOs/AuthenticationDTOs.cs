using System.ComponentModel.DataAnnotations;

namespace RewardStar.Api.DTOs;

// Registration DTOs
public record RegisterRequestDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(320, ErrorMessage = "Email cannot exceed 320 characters")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number")]
    public string Password { get; init; } = string.Empty;
}

public record GoogleRegisterRequestDto
{
    [Required(ErrorMessage = "Google ID token is required")]
    public string IdToken { get; init; } = string.Empty;
}

// Login DTOs
public record LoginRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; init; } = string.Empty;
}

public record GoogleLoginRequestDto
{
    [Required(ErrorMessage = "Google ID token is required")]
    public string IdToken { get; init; } = string.Empty;
}

// Response DTOs
public record AuthResponseDto
{
    public int UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public bool Active { get; init; }
    public string? GoogleAuthId { get; init; }
}

public record UserDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool Active { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public bool IsAdmin { get; init; }
    public bool IsGoogleAccount { get; init; }
}

// User Management DTOs
public record UpdateUserRequestDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(320, ErrorMessage = "Email cannot exceed 320 characters")]
    public string Email { get; init; } = string.Empty;

    [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number")]
    public string? NewPassword { get; init; }

    public string? CurrentPassword { get; init; }
    public bool? Active { get; init; }
}

public record UpdateStatusRequestDto
{
    [Required]
    public bool Active { get; init; }
}
