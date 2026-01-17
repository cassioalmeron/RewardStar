using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RewardStar.Api.DTOs;
using RewardStart.Core;
using System.Security.Claims;
using RewardStar.Api.Extensions;
using RewardStart.Core.Utils;
using RewardStart.Core.Extensions;

namespace RewardStar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly RewardStartDbContext _dbContext;
    private readonly ILogger<UsersController> _logger;

    public UsersController(RewardStartDbContext dbContext, ILogger<UsersController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Get all users (admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        try
        {
            // Load users from database first
            var users = await _dbContext.Users.ToListAsync();

            // Then map to DTOs using the extension method (client-side)
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Active = u.Active,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                IsAdmin = u.IsAdmin()
            }).ToList();

            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new { message = "An error occurred while retrieving users" });
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Active = user.Active,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsAdmin = user.IsAdmin()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            return StatusCode(500, new { message = "An error occurred while retrieving user information" });
        }
    }

    /// <summary>
    /// Get user by ID (admin or self only)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        try
        {
            // Authorization: Admin or requesting own profile
            if (!User.IsAdminOrSelf(id))
                return Forbid();

            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            return Ok(new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Active = user.Active,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsAdmin = user.IsAdmin()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the user" });
        }
    }

    /// <summary>
    /// Update user active status (admin only)
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequestDto request)
    {
        try
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            // Prevent admin from deactivating themselves
            if (User.IsAdmin() && user.IsAdmin() && !request.Active)
                return BadRequest(new { message = "Admin user cannot deactivate themselves" });

            user.Active = request.Active;

            // Explicitly mark as modified due to NoTracking default
            _dbContext.Entry(user).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("User {UserId} status updated to {Status} by admin {AdminId}",
                id, request.Active ? "active" : "inactive", User.GetUserId());

            return Ok(new { message = $"User {(request.Active ? "activated" : "deactivated")} successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user status {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while updating user status" });
        }
    }

    /// <summary>
    /// Update user profile (admin can edit any, users can edit self only)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequestDto request)
    {
        try
        {
            // Authorization: Admin or self
            if (!User.IsAdminOrSelf(id))
                return Forbid();

            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found" });

            // Validate unique email if changed
            if (request.Email != user.Email)
            {
                var emailExists = await _dbContext.Users
                    .AnyAsync(u => u.Email == request.Email && u.Id != id);
                if (emailExists)
                    return Conflict(new { message = "Email already in use" });
            }

            // Update fields
            user.Name = request.Name;
            user.Email = request.Email;

            // Password update (optional)
            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                // If user has existing password, verify current password (unless admin)
                if (!User.IsAdmin() && !string.IsNullOrEmpty(user.Password))
                {
                    if (string.IsNullOrEmpty(request.CurrentPassword))
                        return BadRequest(new { message = "Current password required" });

                    if (!PasswordHasher.VerifyPassword(request.CurrentPassword, user.Password))
                        return BadRequest(new { message = "Current password is incorrect" });
                }

                user.Password = PasswordHasher.HashPassword(request.NewPassword);
            }

            // Prevent admin from deactivating themselves via this endpoint
            if (User.IsAdmin() && user.IsAdmin() && request.Active == false)
                return BadRequest(new { message = "Admin user cannot deactivate themselves" });

            // Only admin can change Active status via this endpoint
            if (request.Active.HasValue && User.IsAdmin())
                user.Active = request.Active.Value;

            // Explicitly mark as modified due to NoTracking default
            _dbContext.Entry(user).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("User {UserId} updated by {ActorId}", id, User.GetUserId());

            return Ok(new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Active = user.Active,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsAdmin = user.IsAdmin()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the user" });
        }
    }
}
