using System.Security.Claims;
using RewardStart.Core.Constants;

namespace RewardStar.Api.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal to handle authorization logic
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Extracts the user ID from JWT claims
    /// </summary>
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    /// <summary>
    /// Checks if the current user is an admin (User ID = 1)
    /// </summary>
    public static bool IsAdmin(this ClaimsPrincipal principal) =>
        principal.GetUserId() == UserConstants.ADMIN_USER_ID;

    /// <summary>
    /// Checks if the current user is an admin or is accessing their own resource
    /// </summary>
    public static bool IsAdminOrSelf(this ClaimsPrincipal principal, int targetUserId)
    {
        var currentUserId = principal.GetUserId();
        return currentUserId == UserConstants.ADMIN_USER_ID || currentUserId == targetUserId;
    }
}
