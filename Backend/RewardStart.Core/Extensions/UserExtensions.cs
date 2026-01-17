using RewardStart.Core.Models;
using RewardStart.Core.Constants;

namespace RewardStart.Core.Extensions;

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
