namespace RewardStar.Api.DTOs;

/// <summary>
/// Base class for all DTOs. Uses a regular class (not record) to ensure
/// reflection-based property setting works correctly with extension methods.
/// </summary>
public class DtoBase
{
    public int Id { get; set; }
}