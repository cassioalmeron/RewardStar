using Microsoft.VisualStudio.TestTools.UnitTesting;
using RewardStar.Api.Controllers;
using RewardStar.Api.DTOs;
using RewardStart.Core.Models;
using System.Reflection;

namespace RewardStar.Tests.Api.Controllers;

[TestClass]
public class ActivitiesControllerTests
{
    /// <summary>
    /// Helper method to invoke the private MapToActivityDto method via reflection
    /// </summary>
    private ActivityDto InvokeMapToActivityDto(Activity activity)
    {
        var method = typeof(ActivitiesController).GetMethod("MapToActivityDto",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            new[] { typeof(Activity) },
            null);

        return (ActivityDto)method?.Invoke(null, new object[] { activity })!;
    }

    /// <summary>
    /// Test that MapToActivityDto correctly maps Activity properties to ActivityDto
    /// Invokes the actual private helper method in ActivitiesController
    /// </summary>
    [TestMethod]
    public void MapToActivityDto_WithValidActivity_CopiesAllProperties()
    {
        // Arrange
        var activity = new Activity
        {
            Id = 5,
            Description = "Daily Workout",
            Level = Level.Medium,
            UserId = 10,
            Position = 1,
            Active = true,
            Monday = true,
            Tuesday = false,
            Wednesday = true,
            Thursday = false,
            Friday = true,
        };

        // Act - Invoke the actual private MapToActivityDto method from ActivitiesController
        var dto = InvokeMapToActivityDto(activity);

        // Assert
        Assert.IsNotNull(dto);
        Assert.AreEqual(5, dto.Id);
        Assert.AreEqual("Daily Workout", dto.Description);
        Assert.AreEqual((int)Level.Medium, dto.Level);
        Assert.AreEqual(10, dto.UserId);
        Assert.AreEqual(1, dto.Position);
        Assert.IsTrue(dto.Active);
        Assert.IsTrue(dto.Monday);
        Assert.IsFalse(dto.Tuesday);
        Assert.IsTrue(dto.Wednesday);
        Assert.IsFalse(dto.Thursday);
        Assert.IsTrue(dto.Friday);
    }

    /// <summary>
    /// Test that multiple activities can be mapped to ActivityDto list correctly
    /// </summary>
    [TestMethod]
    public void MapToActivityDto_WithMultipleActivities_CopiesAllActivitiesToDtos()
    {
        // Arrange
        var activities = new List<Activity>
        {
            new()
            {
                Id = 1,
                Description = "Activity 1",
                Level = Level.Easy,
                UserId = 1,
                Position = 1,
                Active = true,
                Monday = true,
                Tuesday = false,
                Wednesday = false,
                Thursday = false,
                Friday = false
            },
            new()
            {
                Id = 2,
                Description = "Activity 2",
                Level = Level.Hard,
                UserId = 1,
                Position = 2,
                Active = true,
                Monday = false,
                Tuesday = true,
                Wednesday = false,
                Thursday = false,
                Friday = false
            }
        };

        // Act
        var dtos = activities.Select(InvokeMapToActivityDto).ToList();

        // Assert
        Assert.AreEqual(2, dtos.Count);

        Assert.AreEqual("Activity 1", dtos[0].Description);
        Assert.AreEqual((int)Level.Easy, dtos[0].Level);
        Assert.IsTrue(dtos[0].Monday);

        Assert.AreEqual("Activity 2", dtos[1].Description);
        Assert.AreEqual((int)Level.Hard, dtos[1].Level);
        Assert.IsTrue(dtos[1].Tuesday);
    }

    /// <summary>
    /// Test that ActivityDto preserves all day properties from Activity entity
    /// </summary>
    [TestMethod]
    public void MapToActivityDto_WithAllDaysActive_PreservesAllDayFlags()
    {
        // Arrange
        var activity = new Activity
        {
            Id = 10,
            Description = "Full Week Activity",
            Level = Level.Easy,
            UserId = 5,
            Position = 1,
            Active = true,
            Monday = true,
            Tuesday = true,
            Wednesday = true,
            Thursday = true,
            Friday = true,
        };

        // Act
        var dto = InvokeMapToActivityDto(activity);

        // Assert
        Assert.IsTrue(dto.Monday);
        Assert.IsTrue(dto.Tuesday);
        Assert.IsTrue(dto.Wednesday);
        Assert.IsTrue(dto.Thursday);
        Assert.IsTrue(dto.Friday);
    }

    /// <summary>
    /// Test that ActivityDto correctly maps UserId from Activity entity
    /// </summary>
    [TestMethod]
    public void MapToActivityDto_WithDifferentUsers_CorrectlyMapsUserId()
    {
        // Arrange
        var user1Activity = new Activity
        {
            Id = 1,
            Description = "User 1 Activity",
            Level = Level.Easy,
            UserId = 100,
            Position = 1,
            Active = true,
        };

        var user2Activity = new Activity
        {
            Id = 2,
            Description = "User 2 Activity",
            Level = Level.Hard,
            UserId = 200,
            Position = 1,
            Active = true,
        };

        // Act
        var dto1 = InvokeMapToActivityDto(user1Activity);
        var dto2 = InvokeMapToActivityDto(user2Activity);

        // Assert
        Assert.AreEqual(100, dto1.UserId);
        Assert.AreEqual(200, dto2.UserId);
    }

    /// <summary>
    /// Test that ActivityDto handles inactive activities correctly
    /// </summary>
    [TestMethod]
    public void MapToActivityDto_WithInactiveActivity_PreservesActiveFlag()
    {
        // Arrange
        var inactiveActivity = new Activity
        {
            Id = 1,
            Description = "Inactive Activity",
            Level = Level.Medium,
            UserId = 1,
            Position = 1,
            Active = false,
        };

        // Act
        var dto = InvokeMapToActivityDto(inactiveActivity);

        // Assert
        Assert.IsFalse(dto.Active);
    }

    /// <summary>
    /// Test that ActivityDto correctly maps different difficulty levels
    /// </summary>
    [TestMethod]
    public void MapToActivityDto_WithDifferentLevels_CorrectlyMapsLevelEnum()
    {
        // Arrange
        var easyActivity = new Activity { Id = 1, Level = Level.Easy, UserId = 1, Position = 1, Active = true };
        var mediumActivity = new Activity { Id = 2, Level = Level.Medium, UserId = 1, Position = 2, Active = true };
        var hardActivity = new Activity { Id = 3, Level = Level.Hard, UserId = 1, Position = 3, Active = true };

        // Act
        var easyDto = InvokeMapToActivityDto(easyActivity);
        var mediumDto = InvokeMapToActivityDto(mediumActivity);
        var hardDto = InvokeMapToActivityDto(hardActivity);

        // Assert
        Assert.AreEqual((int)Level.Easy, easyDto.Level);
        Assert.AreEqual((int)Level.Medium, mediumDto.Level);
        Assert.AreEqual((int)Level.Hard, hardDto.Level);
    }
}
