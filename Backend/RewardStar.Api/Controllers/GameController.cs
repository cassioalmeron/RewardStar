using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RewardStart.Core;
using RewardStart.Core.Models;
using RewardStar.Api.DTOs;
using RewardStar.Api.Extensions;

namespace RewardStar.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GameController : ControllerBase
{
    private readonly RewardStartDbContext _context;
    private readonly ILogger<GameController> _logger;

    public GameController(RewardStartDbContext context, ILogger<GameController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get the current authenticated user's ID from JWT claims
    /// </summary>
    private int GetCurrentUserId()
    {
        var userId = User.GetUserId();
        if (userId == 0)
            throw new UnauthorizedAccessException("User ID not found in token");

        return userId;
    }

    /// <summary>
    /// Convert Activity entity to ActivityDto response
    /// </summary>
    private static ActivityDto MapToActivityDto(Activity activity)
    {
        return activity.CopyTo<ActivityDto>();
    }

    // GET: api/Game
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetActivities()
    {
        try
        {
            var userId = GetCurrentUserId();

            // Admins should not have access to game activities
            if (User.IsAdmin())
            {
                _logger.LogWarning("Admin user {UserId} attempted to access game", userId);
                return StatusCode(403, new { message = "Admin users cannot access the game" });
            }

            var activities = await _context.Activities
                .Where(x => x.UserId == userId && x.Active)
                .OrderBy(x => x.Position)
                .ToListAsync();

            return Ok(activities.Select(MapToActivityDto));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game activities");
            return StatusCode(500, new { message = "An error occurred while retrieving game activities" });
        }
    }
}
