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

    private static readonly Dictionary<Level, int> POINTS_PER_LEVEL = new()
    {
        { Level.Easy, 1 },
        { Level.Medium, 2 },
        { Level.Hard, 3 }
    };

    private static readonly Dictionary<RewardType, int> REWARD_COSTS = new()
    {
        { RewardType.MMs, 2 },
        { RewardType.Bibs, 4 },
        { RewardType.Caramel, 6 }
    };

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
    private static ActivityDto MapToActivityDto(Activity activity) =>
        activity.CopyTo<ActivityDto>();

    /// <summary>
    /// Parse a day string to GameDayOfWeek enum
    /// </summary>
    private static GameDayOfWeek? ParseDay(string day) =>
        Enum.TryParse<GameDayOfWeek>(day, ignoreCase: true, out var result) ? result : null;

    /// <summary>
    /// Get or create a GameState for the user
    /// </summary>
    private async Task<GameState> GetOrCreateGameStateAsync(int userId)
    {
        var gameState = await _context.GameStates
            .FirstOrDefaultAsync(gs => gs.UserId == userId);

        if (gameState != null)
            return gameState;

        gameState = new GameState
        {
            UserId = userId,
            TotalPoints = 0,
            Balance = 0
        };

        _context.GameStates.Add(gameState);
        await _context.SaveChangesAsync();

        return gameState;
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

    // GET: api/Game/state
    [HttpGet("state")]
    public async Task<ActionResult<GameStateDto>> GetState()
    {
        try
        {
            var userId = GetCurrentUserId();

            if (User.IsAdmin())
                return StatusCode(403, new { message = "Admin users cannot access the game" });

            var gameState = await _context.GameStates
                .FirstOrDefaultAsync(gs => gs.UserId == userId);

            var completions = await _context.GameCompletions
                .Where(gc => gc.UserId == userId && gc.Completed)
                .ToListAsync();

            var completionsDict = new Dictionary<int, CompletionDaysDto>();
            foreach (var completion in completions)
            {
                if (!completionsDict.ContainsKey(completion.ActivityId))
                    completionsDict[completion.ActivityId] = new CompletionDaysDto();

                var days = completionsDict[completion.ActivityId];
                switch (completion.Day)
                {
                    case GameDayOfWeek.Monday: days.Monday = true; break;
                    case GameDayOfWeek.Tuesday: days.Tuesday = true; break;
                    case GameDayOfWeek.Wednesday: days.Wednesday = true; break;
                    case GameDayOfWeek.Thursday: days.Thursday = true; break;
                    case GameDayOfWeek.Friday: days.Friday = true; break;
                }
            }

            return Ok(new GameStateDto
            {
                Completions = completionsDict,
                TotalPoints = gameState?.TotalPoints ?? 0,
                Balance = gameState?.Balance ?? 0
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving game state");
            return StatusCode(500, new { message = "An error occurred while retrieving game state" });
        }
    }

    // POST: api/Game/toggle
    [HttpPost("toggle")]
    public async Task<ActionResult<ToggleCompletionResponseDto>> Toggle([FromBody] ToggleCompletionRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();

            if (User.IsAdmin())
                return StatusCode(403, new { message = "Admin users cannot access the game" });

            var parsedDay = ParseDay(request.Day);
            if (parsedDay == null)
                return BadRequest(new { message = $"Invalid day: {request.Day}. Must be monday, tuesday, wednesday, thursday, or friday" });

            // Verify the activity exists and belongs to the user
            var activity = await _context.Activities
                .FirstOrDefaultAsync(a => a.Id == request.ActivityId && a.UserId == userId);

            if (activity == null)
                return NotFound(new { message = "Activity not found" });

            var points = POINTS_PER_LEVEL.GetValueOrDefault(activity.Level, 0);

            // Find existing completion record
            var completion = await _context.GameCompletions
                .FirstOrDefaultAsync(gc =>
                    gc.ActivityId == request.ActivityId &&
                    gc.Day == parsedDay.Value &&
                    gc.UserId == userId);

            var gameState = await GetOrCreateGameStateAsync(userId);
            _context.Entry(gameState).State = EntityState.Modified;

            bool isNowCompleted;

            if (completion == null)
            {
                // Create new completion (toggling ON)
                completion = new GameCompletion
                {
                    ActivityId = request.ActivityId,
                    Day = parsedDay.Value,
                    Completed = true,
                    UserId = userId
                };
                _context.GameCompletions.Add(completion);
                gameState.TotalPoints += points;
                gameState.Balance += points;
                isNowCompleted = true;
            }
            else if (!completion.Completed)
            {
                // Toggle ON
                _context.Entry(completion).State = EntityState.Modified;
                completion.Completed = true;
                gameState.TotalPoints += points;
                gameState.Balance += points;
                isNowCompleted = true;
            }
            else
            {
                // Toggle OFF
                _context.Entry(completion).State = EntityState.Modified;
                completion.Completed = false;
                gameState.TotalPoints -= points;
                gameState.Balance -= points;
                isNowCompleted = false;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} toggled activity {ActivityId} on {Day} to {Completed}",
                userId, request.ActivityId, request.Day, isNowCompleted);

            return Ok(new ToggleCompletionResponseDto
            {
                Completed = isNowCompleted,
                TotalPoints = gameState.TotalPoints,
                Balance = gameState.Balance
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling game completion");
            return StatusCode(500, new { message = "An error occurred while toggling completion" });
        }
    }

    // POST: api/Game/claim
    [HttpPost("claim")]
    public async Task<ActionResult<ClaimRewardResponseDto>> ClaimReward([FromBody] ClaimRewardRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();

            if (User.IsAdmin())
                return StatusCode(403, new { message = "Admin users cannot access the game" });

            if (!Enum.IsDefined(typeof(RewardType), request.RewardType))
                return BadRequest(new { message = $"Invalid reward type: {request.RewardType}" });

            var rewardType = (RewardType)request.RewardType;
            var cost = REWARD_COSTS[rewardType];

            var gameState = await GetOrCreateGameStateAsync(userId);

            if (gameState.Balance < cost)
                return BadRequest(new { message = $"Insufficient balance. Required: {cost}, available: {gameState.Balance}" });

            _context.Entry(gameState).State = EntityState.Modified;
            gameState.Balance -= cost;

            var rewardClaim = new RewardClaim
            {
                UserId = userId,
                RewardType = rewardType,
                PointsCost = cost,
                ClaimedAt = DateTime.UtcNow
            };
            _context.RewardClaims.Add(rewardClaim);

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} claimed reward {RewardType} for {Cost} points",
                userId, rewardType, cost);

            return Ok(new ClaimRewardResponseDto
            {
                Success = true,
                Balance = gameState.Balance,
                RewardType = request.RewardType,
                PointsCost = cost
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error claiming reward");
            return StatusCode(500, new { message = "An error occurred while claiming reward" });
        }
    }

    // POST: api/Game/reset
    [HttpPost("reset")]
    public async Task<ActionResult> Reset()
    {
        try
        {
            var userId = GetCurrentUserId();

            if (User.IsAdmin())
                return StatusCode(403, new { message = "Admin users cannot access the game" });

            // Delete all completions for the user
            var completions = await _context.GameCompletions
                .Where(gc => gc.UserId == userId)
                .ToListAsync();

            _context.GameCompletions.RemoveRange(completions);

            // Reset game state
            var gameState = await _context.GameStates
                .FirstOrDefaultAsync(gs => gs.UserId == userId);

            if (gameState != null)
            {
                _context.Entry(gameState).State = EntityState.Modified;
                gameState.TotalPoints = 0;
                gameState.Balance = 0;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} reset their game", userId);

            return Ok(new { message = "Game reset successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting game");
            return StatusCode(500, new { message = "An error occurred while resetting the game" });
        }
    }
}
