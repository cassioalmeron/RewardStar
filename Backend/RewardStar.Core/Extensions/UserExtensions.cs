using RewardStar.Core.Models;
using RewardStar.Core.Constants;

namespace RewardStar.Core.Extensions;

/// <summary>
/// Extension methods for the User entity
/// </summary>
public static class UserExtensions
{
    /// <summary>
    /// Checks if the user is an admin (User ID = 1)
    /// </summary>
    public static bool IsAdmin(this User user) => 
        user.Id == UserConstants.ADMIN_USER_ID;
}
