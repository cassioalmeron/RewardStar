using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RewardStar.Api.Controllers;
using RewardStar.Api.DTOs;
using RewardStart.Core;
using RewardStart.Core.Models;
using System.Security.Claims;

namespace RewardStar.Tests.Api.Controllers;

[TestClass]
[DoNotParallelize] // Tests use environment variables to configure DbContext, cannot run in parallel
public class GameControllerTests
{
    private const int TEST_USER_ID = 2; // Non-admin user (admin is 1)

    private static RewardStartDbContext CreateTestDbContext()
    {
        // Use a unique temp file per test to ensure isolation
        var dbPath = Path.Combine(Path.GetTempPath(), $"RewardStarTest_{Guid.NewGuid()}.db");
        Environment.SetEnvironmentVariable("DB_PATH", dbPath);
        Environment.SetEnvironmentVariable("DATABASE_URL", null);

        var context = new RewardStartDbContext();
        context.Database.EnsureCreated();

        // Seed the test user to satisfy FK constraints
        context.Users.Add(new User
        {
            Id = TEST_USER_ID,
            Name = "Test User",
            Email = "test@rewardstar.com",
            Active = true,
            CreatedAt = DateTime.UtcNow
        });
        context.SaveChanges();
        DetachAll(context);

        return context;
    }

    /// <summary>
    /// Detach all tracked entities to prevent tracking conflicts.
    /// Required because Add/Update operations track entities even with NoTracking query behavior.
    /// </summary>
    private static void DetachAll(RewardStartDbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries().ToList())
            entry.State = EntityState.Detached;
    }

    private static GameController CreateController(RewardStartDbContext context)
    {
        var logger = new Mock<ILogger<GameController>>();
        var controller = new GameController(context, logger.Object);

        // Set up ClaimsPrincipal with test user ID
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, TEST_USER_ID.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        return controller;
    }

    private static async Task<Activity> SeedActivityAsync(RewardStartDbContext context, Level level, int userId = TEST_USER_ID)
    {
        var activity = new Activity
        {
            Description = $"Test Activity {level}",
            Level = level,
            UserId = userId,
            Position = 1,
            Active = true,
            Monday = true,
            Tuesday = true,
            Wednesday = true,
            Thursday = true,
            Friday = true
        };

        context.Activities.Add(activity);
        await context.SaveChangesAsync();
        DetachAll(context);
        return activity;
    }

    private static async Task<GameState> SeedGameStateAsync(RewardStartDbContext context, int totalPoints, int balance, int userId = TEST_USER_ID)
    {
        var gameState = new GameState
        {
            UserId = userId,
            TotalPoints = totalPoints,
            Balance = balance
        };

        context.GameStates.Add(gameState);
        await context.SaveChangesAsync();
        DetachAll(context);
        return gameState;
    }

    private static async Task<GameCompletion> SeedCompletionAsync(RewardStartDbContext context, int activityId, GameDayOfWeek day, int userId = TEST_USER_ID)
    {
        var completion = new GameCompletion
        {
            ActivityId = activityId,
            Day = day,
            Completed = true,
            UserId = userId
        };

        context.GameCompletions.Add(completion);
        await context.SaveChangesAsync();
        DetachAll(context);
        return completion;
    }

    // ==================== GetState Tests ====================

    [TestMethod]
    public async Task GetState_WithExistingProgress_ReturnsCompletionsPointsAndBalance()
    {
        // Arrange
        using var context = CreateTestDbContext();
        var activity = await SeedActivityAsync(context, Level.Medium);
        await SeedGameStateAsync(context, totalPoints: 4, balance: 2);
        await SeedCompletionAsync(context, activity.Id, GameDayOfWeek.Monday);
        await SeedCompletionAsync(context, activity.Id, GameDayOfWeek.Wednesday);

        var controller = CreateController(context);

        // Act
        var result = await controller.GetState();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var state = okResult.Value as GameStateDto;
        Assert.IsNotNull(state);
        Assert.AreEqual(4, state.TotalPoints);
        Assert.AreEqual(2, state.Balance);
        Assert.IsTrue(state.Completions.ContainsKey(activity.Id));
        Assert.IsTrue(state.Completions[activity.Id].Monday);
        Assert.IsFalse(state.Completions[activity.Id].Tuesday);
        Assert.IsTrue(state.Completions[activity.Id].Wednesday);
    }

    [TestMethod]
    public async Task GetState_WithNoProgress_ReturnsEmptyState()
    {
        // Arrange
        using var context = CreateTestDbContext();
        var controller = CreateController(context);

        // Act
        var result = await controller.GetState();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var state = okResult.Value as GameStateDto;
        Assert.IsNotNull(state);
        Assert.AreEqual(0, state.TotalPoints);
        Assert.AreEqual(0, state.Balance);
        Assert.AreEqual(0, state.Completions.Count);
    }

    // ==================== Toggle Tests ====================

    [TestMethod]
    public async Task Toggle_CompletingEasyActivity_AddsOnePoint()
    {
        // Arrange
        using var context = CreateTestDbContext();
        var activity = await SeedActivityAsync(context, Level.Easy);
        var controller = CreateController(context);

        // Act
        var result = await controller.Toggle(new ToggleCompletionRequestDto
        {
            ActivityId = activity.Id,
            Day = "monday"
        });

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var response = okResult.Value as ToggleCompletionResponseDto;
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Completed);
        Assert.AreEqual(1, response.TotalPoints);
        Assert.AreEqual(1, response.Balance);
    }

    [TestMethod]
    public async Task Toggle_CompletingMediumActivity_AddsTwoPoints()
    {
        // Arrange
        using var context = CreateTestDbContext();
        var activity = await SeedActivityAsync(context, Level.Medium);
        var controller = CreateController(context);

        // Act
        var result = await controller.Toggle(new ToggleCompletionRequestDto
        {
            ActivityId = activity.Id,
            Day = "tuesday"
        });

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var response = okResult.Value as ToggleCompletionResponseDto;
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Completed);
        Assert.AreEqual(2, response.TotalPoints);
        Assert.AreEqual(2, response.Balance);
    }

    [TestMethod]
    public async Task Toggle_CompletingHardActivity_AddsThreePoints()
    {
        // Arrange
        using var context = CreateTestDbContext();
        var activity = await SeedActivityAsync(context, Level.Hard);
        var controller = CreateController(context);

        // Act
        var result = await controller.Toggle(new ToggleCompletionRequestDto
        {
            ActivityId = activity.Id,
            Day = "friday"
        });

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var response = okResult.Value as ToggleCompletionResponseDto;
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Completed);
        Assert.AreEqual(3, response.TotalPoints);
        Assert.AreEqual(3, response.Balance);
    }

    [TestMethod]
    public async Task Toggle_UncompletingActivity_SubtractsPointsAndBalance()
    {
        // Arrange
        using var context = CreateTestDbContext();
        var activity = await SeedActivityAsync(context, Level.Medium);
        await SeedGameStateAsync(context, totalPoints: 2, balance: 2);
        await SeedCompletionAsync(context, activity.Id, GameDayOfWeek.Monday);

        var controller = CreateController(context);

        // Act - toggle OFF the existing completion
        var result = await controller.Toggle(new ToggleCompletionRequestDto
        {
            ActivityId = activity.Id,
            Day = "monday"
        });

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var response = okResult.Value as ToggleCompletionResponseDto;
        Assert.IsNotNull(response);
        Assert.IsFalse(response.Completed);
        Assert.AreEqual(0, response.TotalPoints);
        Assert.AreEqual(0, response.Balance);
    }

    [TestMethod]
    public async Task Toggle_ActivityDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var context = CreateTestDbContext();
        var controller = CreateController(context);

        // Act
        var result = await controller.Toggle(new ToggleCompletionRequestDto
        {
            ActivityId = 999,
            Day = "monday"
        });

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
    }

    [TestMethod]
    public async Task Toggle_ActivityBelongsToOtherUser_ReturnsNotFound()
    {
        // Arrange
        using var context = CreateTestDbContext();
        var otherUserId = 99;

        // Seed a user for FK constraint
        context.Users.Add(new User
        {
            Id = otherUserId,
            Name = "Other User",
            Email = "other@test.com",
            Active = true,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        DetachAll(context);

        var activity = await SeedActivityAsync(context, Level.Easy, userId: otherUserId);
        var controller = CreateController(context);

        // Act - TEST_USER_ID tries to toggle other user's activity
        var result = await controller.Toggle(new ToggleCompletionRequestDto
        {
            ActivityId = activity.Id,
            Day = "monday"
        });

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
    }

    // ==================== ClaimReward Tests ====================

    [TestMethod]
    public async Task ClaimReward_WithSufficientBalance_DeductsCorrectPoints()
    {
        // Arrange
        using var context = CreateTestDbContext();
        await SeedGameStateAsync(context, totalPoints: 10, balance: 10);
        var controller = CreateController(context);

        // Act - Claim Bibs (cost: 4)
        var result = await controller.ClaimReward(new ClaimRewardRequestDto
        {
            RewardType = (int)RewardType.Bibs
        });

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var response = okResult.Value as ClaimRewardResponseDto;
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.AreEqual(6, response.Balance); // 10 - 4 = 6
        Assert.AreEqual((int)RewardType.Bibs, response.RewardType);
        Assert.AreEqual(4, response.PointsCost);
    }

    [TestMethod]
    public async Task ClaimReward_WithInsufficientBalance_ReturnsBadRequest()
    {
        // Arrange
        using var context = CreateTestDbContext();
        await SeedGameStateAsync(context, totalPoints: 3, balance: 1);
        var controller = CreateController(context);

        // Act - Claim MMs (cost: 2) but only have 1 balance
        var result = await controller.ClaimReward(new ClaimRewardRequestDto
        {
            RewardType = (int)RewardType.MMs
        });

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
    }

    [TestMethod]
    public async Task ClaimReward_CreatesRewardClaimRecord_WithCorrectRewardType()
    {
        // Arrange
        using var context = CreateTestDbContext();
        await SeedGameStateAsync(context, totalPoints: 10, balance: 10);
        var controller = CreateController(context);

        // Act - Claim Caramel (cost: 6)
        await controller.ClaimReward(new ClaimRewardRequestDto
        {
            RewardType = (int)RewardType.Caramel
        });

        // Assert - Verify the RewardClaim was persisted
        var claims = await context.RewardClaims
            .Where(rc => rc.UserId == TEST_USER_ID)
            .ToListAsync();

        Assert.AreEqual(1, claims.Count);
        Assert.AreEqual(RewardType.Caramel, claims[0].RewardType);
        Assert.AreEqual(6, claims[0].PointsCost);
        Assert.AreEqual(TEST_USER_ID, claims[0].UserId);
    }

    // ==================== Reset Tests ====================

    [TestMethod]
    public async Task Reset_WithExistingProgress_ClearsCompletionsAndResetsPointsBalance()
    {
        // Arrange
        using var context = CreateTestDbContext();
        var activity = await SeedActivityAsync(context, Level.Medium);
        await SeedGameStateAsync(context, totalPoints: 8, balance: 4);
        await SeedCompletionAsync(context, activity.Id, GameDayOfWeek.Monday);
        await SeedCompletionAsync(context, activity.Id, GameDayOfWeek.Tuesday);

        var controller = CreateController(context);

        // Act
        var result = await controller.Reset();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);

        // Verify completions are deleted
        var completions = await context.GameCompletions
            .Where(gc => gc.UserId == TEST_USER_ID)
            .ToListAsync();
        Assert.AreEqual(0, completions.Count);

        // Verify game state is reset
        var gameState = await context.GameStates
            .FirstOrDefaultAsync(gs => gs.UserId == TEST_USER_ID);
        Assert.IsNotNull(gameState);
        Assert.AreEqual(0, gameState.TotalPoints);
        Assert.AreEqual(0, gameState.Balance);
    }

    [TestMethod]
    public async Task Reset_WithNoGameState_CompletesSuccessfully()
    {
        // Arrange
        using var context = CreateTestDbContext();
        var controller = CreateController(context);

        // Act
        var result = await controller.Reset();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
    }
}
