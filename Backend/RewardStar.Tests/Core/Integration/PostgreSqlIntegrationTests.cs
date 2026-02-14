using Microsoft.EntityFrameworkCore;
using RewardStar.Core;
using RewardStar.Core.Models;
using Testcontainers.PostgreSql;

namespace RewardStar.Tests.Core.Integration;

/// <summary>
/// Integration tests that run against a real PostgreSQL container via Testcontainers.
/// These tests validate that EF Core migrations apply correctly on PostgreSQL and that
/// basic CRUD operations work — catching issues like missing Npgsql identity annotations
/// that SQLite tests cannot detect.
///
/// Requires Docker to be running. Filter with: dotnet test --filter TestCategory=Integration
/// </summary>
[TestClass]
[TestCategory("Integration")]
[DoNotParallelize]
public class PostgreSqlIntegrationTests
{
    private static PostgreSqlContainer _postgresContainer = null!;
    private static string _connectionString = null!;

    [ClassInitialize]
    public static async Task ClassInit(TestContext context)
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("rewardstar_test")
            .Build();

        await _postgresContainer.StartAsync();
        _connectionString = _postgresContainer.GetConnectionString();
    }

    [ClassCleanup]
    public static async Task ClassClean()
    {
        if (_postgresContainer != null)
            await _postgresContainer.DisposeAsync();
    }

    [TestInitialize]
    public void TestInit()
    {
        using var context = CreatePostgreSqlContext();
        context.Database.EnsureDeleted();
        context.Database.Migrate();
    }

    private static RewardStarDbContext CreatePostgreSqlContext()
    {
        Environment.SetEnvironmentVariable("DATABASE_URL", _connectionString);
        Environment.SetEnvironmentVariable("DB_PATH", null);
        return new RewardStarDbContext();
    }

    private static void DetachAll(RewardStarDbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries().ToList())
            entry.State = EntityState.Detached;
    }

    private static User SeedTestUser(RewardStarDbContext context)
    {
        var user = new User
        {
            Name = "Integration Test User",
            Email = $"integration-{Guid.NewGuid()}@test.com",
            Active = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        context.SaveChanges();
        DetachAll(context);
        return user;
    }

    private static Activity SeedActivity(RewardStarDbContext context, int userId)
    {
        var activity = new Activity
        {
            Description = "Integration Test Activity",
            Level = Level.Medium,
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
        context.SaveChanges();
        DetachAll(context);
        return activity;
    }

    // ==================== Migration Tests ====================

    [TestMethod]
    public void Migrate_AllMigrations_ApplySuccessfully()
    {
        using var context = CreatePostgreSqlContext();

        // If migrations have issues (e.g., missing Npgsql annotations),
        // this would have already failed in TestInitialize.
        // Verify all tables are queryable.
        Assert.IsNotNull(context.Users.ToList());
        Assert.IsNotNull(context.Activities.ToList());
        Assert.IsNotNull(context.GameStates.ToList());
        Assert.IsNotNull(context.GameCompletions.ToList());
        Assert.IsNotNull(context.RewardClaims.ToList());
    }

    [TestMethod]
    public void AutoIncrementIds_WorkForAllGameEntities()
    {
        using var context = CreatePostgreSqlContext();
        var user = SeedTestUser(context);

        // GameState — auto-increment Id
        var gameState = new GameState { UserId = user.Id, TotalPoints = 0, Balance = 0 };
        context.GameStates.Add(gameState);
        context.SaveChanges();
        Assert.IsTrue(gameState.Id > 0, "GameState.Id should be auto-generated");
        DetachAll(context);

        // Activity — auto-increment Id
        var activity = SeedActivity(context, user.Id);
        Assert.IsTrue(activity.Id > 0, "Activity.Id should be auto-generated");

        // GameCompletion — auto-increment Id
        var completion = new GameCompletion
        {
            ActivityId = activity.Id,
            Day = GameDayOfWeek.Monday,
            Completed = true,
            UserId = user.Id
        };
        context.GameCompletions.Add(completion);
        context.SaveChanges();
        Assert.IsTrue(completion.Id > 0, "GameCompletion.Id should be auto-generated");
        DetachAll(context);

        // RewardClaim — auto-increment Id
        var claim = new RewardClaim
        {
            UserId = user.Id,
            RewardType = RewardType.MMs,
            PointsCost = 2,
            ClaimedAt = DateTime.UtcNow
        };
        context.RewardClaims.Add(claim);
        context.SaveChanges();
        Assert.IsTrue(claim.Id > 0, "RewardClaim.Id should be auto-generated");
    }

    // ==================== GameState CRUD ====================

    [TestMethod]
    public void GameState_Create_Persists()
    {
        using var context = CreatePostgreSqlContext();
        var user = SeedTestUser(context);

        var gameState = new GameState { UserId = user.Id, TotalPoints = 10, Balance = 5 };
        context.GameStates.Add(gameState);
        context.SaveChanges();
        DetachAll(context);

        var retrieved = context.GameStates.FirstOrDefault(gs => gs.UserId == user.Id);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(10, retrieved.TotalPoints);
        Assert.AreEqual(5, retrieved.Balance);
    }

    [TestMethod]
    public void GameState_Update_ModifiesPointsAndBalance()
    {
        using var context = CreatePostgreSqlContext();
        var user = SeedTestUser(context);

        var gameState = new GameState { UserId = user.Id, TotalPoints = 5, Balance = 5 };
        context.GameStates.Add(gameState);
        context.SaveChanges();
        DetachAll(context);

        // Update
        gameState.TotalPoints = 15;
        gameState.Balance = 10;
        context.Entry(gameState).State = EntityState.Modified;
        context.SaveChanges();
        DetachAll(context);

        var retrieved = context.GameStates.FirstOrDefault(gs => gs.UserId == user.Id);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(15, retrieved.TotalPoints);
        Assert.AreEqual(10, retrieved.Balance);
    }

    [TestMethod]
    public void GameState_Delete_Removes()
    {
        using var context = CreatePostgreSqlContext();
        var user = SeedTestUser(context);

        var gameState = new GameState { UserId = user.Id, TotalPoints = 5, Balance = 5 };
        context.GameStates.Add(gameState);
        context.SaveChanges();
        DetachAll(context);

        context.GameStates.Remove(gameState);
        context.SaveChanges();
        DetachAll(context);

        var retrieved = context.GameStates.FirstOrDefault(gs => gs.UserId == user.Id);
        Assert.IsNull(retrieved);
    }

    // ==================== GameCompletion CRUD ====================

    [TestMethod]
    public void GameCompletion_Create_Persists()
    {
        using var context = CreatePostgreSqlContext();
        var user = SeedTestUser(context);
        var activity = SeedActivity(context, user.Id);

        var completion = new GameCompletion
        {
            ActivityId = activity.Id,
            Day = GameDayOfWeek.Wednesday,
            Completed = true,
            UserId = user.Id
        };
        context.GameCompletions.Add(completion);
        context.SaveChanges();
        DetachAll(context);

        var retrieved = context.GameCompletions.FirstOrDefault(gc => gc.Id == completion.Id);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(activity.Id, retrieved.ActivityId);
        Assert.AreEqual(GameDayOfWeek.Wednesday, retrieved.Day);
        Assert.IsTrue(retrieved.Completed);
    }

    [TestMethod]
    public void GameCompletion_Update_TogglesCompleted()
    {
        using var context = CreatePostgreSqlContext();
        var user = SeedTestUser(context);
        var activity = SeedActivity(context, user.Id);

        var completion = new GameCompletion
        {
            ActivityId = activity.Id,
            Day = GameDayOfWeek.Monday,
            Completed = true,
            UserId = user.Id
        };
        context.GameCompletions.Add(completion);
        context.SaveChanges();
        DetachAll(context);

        // Toggle off
        completion.Completed = false;
        context.Entry(completion).State = EntityState.Modified;
        context.SaveChanges();
        DetachAll(context);

        var retrieved = context.GameCompletions.FirstOrDefault(gc => gc.Id == completion.Id);
        Assert.IsNotNull(retrieved);
        Assert.IsFalse(retrieved.Completed);
    }

    [TestMethod]
    public void GameCompletion_Delete_Removes()
    {
        using var context = CreatePostgreSqlContext();
        var user = SeedTestUser(context);
        var activity = SeedActivity(context, user.Id);

        var completion = new GameCompletion
        {
            ActivityId = activity.Id,
            Day = GameDayOfWeek.Tuesday,
            Completed = true,
            UserId = user.Id
        };
        context.GameCompletions.Add(completion);
        context.SaveChanges();
        var completionId = completion.Id;
        DetachAll(context);

        context.GameCompletions.Remove(completion);
        context.SaveChanges();
        DetachAll(context);

        var retrieved = context.GameCompletions.FirstOrDefault(gc => gc.Id == completionId);
        Assert.IsNull(retrieved);
    }

    [TestMethod]
    public void GameCompletion_UniqueConstraint_PreventsDuplicates()
    {
        using var context = CreatePostgreSqlContext();
        var user = SeedTestUser(context);
        var activity = SeedActivity(context, user.Id);

        var completion1 = new GameCompletion
        {
            ActivityId = activity.Id,
            Day = GameDayOfWeek.Monday,
            Completed = true,
            UserId = user.Id
        };
        context.GameCompletions.Add(completion1);
        context.SaveChanges();
        DetachAll(context);

        // Attempt duplicate (same ActivityId + Day + UserId)
        var completion2 = new GameCompletion
        {
            ActivityId = activity.Id,
            Day = GameDayOfWeek.Monday,
            Completed = false,
            UserId = user.Id
        };
        context.GameCompletions.Add(completion2);

        Assert.ThrowsExactly<DbUpdateException>(() => context.SaveChanges());
    }

    // ==================== RewardClaim CRUD ====================

    [TestMethod]
    public void RewardClaim_Create_PersistsWithCorrectDateTime()
    {
        using var context = CreatePostgreSqlContext();
        var user = SeedTestUser(context);

        var claimedAt = DateTime.UtcNow;
        var claim = new RewardClaim
        {
            UserId = user.Id,
            RewardType = RewardType.Caramel,
            PointsCost = 6,
            ClaimedAt = claimedAt
        };
        context.RewardClaims.Add(claim);
        context.SaveChanges();
        DetachAll(context);

        var retrieved = context.RewardClaims.FirstOrDefault(rc => rc.Id == claim.Id);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(RewardType.Caramel, retrieved.RewardType);
        Assert.AreEqual(6, retrieved.PointsCost);
        Assert.AreEqual(user.Id, retrieved.UserId);
        // DateTime should be preserved as UTC within a small tolerance
        Assert.IsTrue(Math.Abs((retrieved.ClaimedAt - claimedAt).TotalSeconds) < 1);
    }

    [TestMethod]
    public void RewardClaim_Delete_Removes()
    {
        using var context = CreatePostgreSqlContext();
        var user = SeedTestUser(context);

        var claim = new RewardClaim
        {
            UserId = user.Id,
            RewardType = RewardType.Bibs,
            PointsCost = 4,
            ClaimedAt = DateTime.UtcNow
        };
        context.RewardClaims.Add(claim);
        context.SaveChanges();
        var claimId = claim.Id;
        DetachAll(context);

        context.RewardClaims.Remove(claim);
        context.SaveChanges();
        DetachAll(context);

        var retrieved = context.RewardClaims.FirstOrDefault(rc => rc.Id == claimId);
        Assert.IsNull(retrieved);
    }
}
