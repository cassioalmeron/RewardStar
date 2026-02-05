using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RewardStart.Core;
using RewardStart.Core.Models;
using System.Reflection;
using RewardStar.Api.DTOs;
using RewardStar.Api.Extensions;

namespace RewardStar.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ActivitiesController : ControllerBase
{
    private readonly RewardStartDbContext _context;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(RewardStartDbContext context, ILogger<ActivitiesController> logger)
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

    private bool HasChanges(Activity newActivity, Activity existingActivity)
    {
        var properties = typeof(Activity).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        return properties.Any(prop =>
        {
            var newValue = prop.GetValue(newActivity);
            var existingValue = prop.GetValue(existingActivity);

            // Ignore Id and navigation properties
            if (prop.Name == nameof(EntityBase.Id) || prop.Name == nameof(Activity.User))
                return false;

            return !Equals(newValue, existingValue);
        });
    }

    /// <summary>
    /// Get all activities for the current authenticated user
    /// </summary>
    /// <returns>List of user's activities ordered by position</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetActivities()
    {
        try
        {
            var userId = GetCurrentUserId();

            // Admins should not have activities
            if (User.IsAdmin())
            {
                _logger.LogWarning("Admin user {UserId} attempted to access activities", userId);
                return StatusCode(403, new { message = "Admin users cannot manage activities" });
            }

            var activities = await _context.Activities
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.Position)
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
            _logger.LogError(ex, "Error retrieving activities");
            return StatusCode(500, new { message = "An error occurred while retrieving activities" });
        }
    }

    /// <summary>
    /// Create, update, or delete activities for the current authenticated user
    /// </summary>
    /// <param name="activities">List of activities to sync</param>
    /// <returns>Updated list of user's activities</returns>
    [HttpPost]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> PostActivity(IEnumerable<Activity> activities)
    {
        try
        {
            var userId = GetCurrentUserId();

            // Admins should not have activities
            if (User.IsAdmin())
            {
                _logger.LogWarning("Admin user {UserId} attempted to modify activities", userId);
                return StatusCode(403, new { message = "Admin users cannot manage activities" });
            }

            if (!activities.Any())
                return BadRequest(new { message = "No activities provided" });

            // Get only the current user's existing activities
            var existingActivities = await _context.Activities
                .Where(a => a.UserId == userId)
                .ToListAsync();

            // Assign positions and ensure UserId is set
            var activitiesWithPosition = activities.Select((activity, index) =>
            {
                activity.Position = index + 1;
                activity.UserId = userId;  // Force the correct UserId
                return activity;
            }).ToList();

            // Determine which activities to add, update, or delete
            var newActivities = activitiesWithPosition
                .Where(a => existingActivities.All(ea => ea.Id != a.Id))
                .ToList();

            var updatedActivities = activitiesWithPosition
                .Where(a => existingActivities.Any(ea => ea.Id == a.Id))
                .ToList();

            var activitiesToDelete = existingActivities
                .Where(ea => !activitiesWithPosition.Any(a => a.Id == ea.Id))
                .ToList();

            // Add new activities
            await _context.Activities.AddRangeAsync(newActivities);

            // Update existing activities (only if changes detected)
            foreach (var activity in updatedActivities)
            {
                var existingActivity = existingActivities.First(ea => ea.Id == activity.Id);

                // Security check: Verify the existing activity belongs to the user
                if (existingActivity.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to update activity {ActivityId} owned by user {OwnerId}",
                        userId, existingActivity.Id, existingActivity.UserId);
                    continue;  // Skip this activity
                }

                if (HasChanges(activity, existingActivity))
                {
                    _context.Entry(existingActivity).CurrentValues.SetValues(activity);
                    _context.Entry(existingActivity).State = EntityState.Modified;
                }
            }

            // Delete removed activities (already filtered by user)
            _context.Activities.RemoveRange(activitiesToDelete);

            await _context.SaveChangesAsync();

            // Return updated list of user's activities
            var result = await _context.Activities
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.Position)
                .ToListAsync();

            _logger.LogInformation("User {UserId} synced {Count} activities", userId, result.Count);

            return Ok(result.Select(MapToActivityDto));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { message = ex.Message });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating activities");
            return StatusCode(409, new { message = "Concurrency conflict. Please refresh and try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing activities");
            return StatusCode(500, new { message = "An error occurred while syncing activities" });
        }
    }
}
