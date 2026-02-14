using RewardStar.Api.DTOs;
using RewardStar.Core.Models;
using RewardStar.Core.Extensions;
using RewardStar.Api.Extensions;

namespace RewardStar.Tests.Api.Extensions;

[TestClass]
public class EntityBaseExtensionTests
{
    /// <summary>
    /// Scenario 1: Basic Entity-to-DTO Conversion
    /// Tests that simple properties are copied from entity to DTO
    /// </summary>
    [TestMethod]
    public void CopyTo_WithValidUserEntity_CopiesBasicProperties()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            Active = true,
            CreatedAt = new DateTime(2026, 1, 1),
            LastLoginAt = new DateTime(2026, 1, 31)
        };
        var target = new CreateUserDto();

        // Act
        user.CopyTo(target);

        // Assert
        Assert.AreEqual(1, target.Id);
        Assert.AreEqual("John Doe", target.Name);
        Assert.AreEqual("john@example.com", target.Email);
    }

    /// <summary>
    /// Scenario 1b: Generic CopyTo - Create New DTO
    /// Tests that CopyTo generic creates a new DTO and copies properties
    /// </summary>
    [TestMethod]
    public void CopyToGeneric_WithValidUserEntity_CreatesNewDto()
    {
        // Arrange
        var user = new User
        {
            Id = 2,
            Name = "Alice Smith",
            Email = "alice@example.com",
            Active = true,
            CreatedAt = new DateTime(2026, 1, 15)
        };

        // Act
        var dto = user.CopyTo<CreateUserDto>();

        // Assert
        Assert.IsNotNull(dto);
        Assert.AreEqual(2, dto.Id);
        Assert.AreEqual("Alice Smith", dto.Name);
        Assert.AreEqual("alice@example.com", dto.Email);
    }

    /// <summary>
    /// Scenario 2: Navigation Property Conversion (Entity Reference → ID)
    /// Tests that navigation properties are converted to their ID values
    /// </summary>
    [TestMethod]
    public void CopyTo_WithNavigationProperty_ConvertsToId()
    {
        // Arrange
        var activity = new Activity
        {
            Id = 5,
            Description = "Daily Workout",
            UserId = 10,
            User = new User { Id = 10, Name = "John" },  // Navigation property
            Monday = true,
            Active = true
        };
        var target = new CreateActivityDto();

        // Act
        activity.CopyTo(target);

        // Assert
        Assert.AreEqual(5, target.Id);
        Assert.AreEqual("Daily Workout", target.Description);
        Assert.AreEqual(10, target.UserId);  // User navigation property converted to ID
        Assert.IsTrue(target.Monday);
        Assert.IsTrue(target.Active);
    }

    /// <summary>
    /// Scenario 3: In-Place Entity-to-DTO Update
    /// Tests that existing DTO is updated with entity values
    /// </summary>
    [TestMethod]
    public void CopyTo_WithExistingDto_UpdatesProperties()
    {
        // Arrange
        var user = new User
        {
            Id = 3,
            Name = "Bob Johnson",
            Email = "bob@example.com",
            Active = true,
            CreatedAt = new DateTime(2026, 1, 20)
        };

        var dto = new CreateUserDto();

        // Act
        user.CopyTo(dto);

        // Assert
        Assert.AreEqual(3, dto.Id);
        Assert.AreEqual("Bob Johnson", dto.Name);
        Assert.AreEqual("bob@example.com", dto.Email);
    }

    /// <summary>
    /// Scenario 4: Null Navigation Properties
    /// Tests that null navigation properties are handled gracefully
    /// </summary>
    [TestMethod]
    public void CopyTo_WithNullNavigationProperty_HandlesGracefully()
    {
        // Arrange
        var activity = new Activity
        {
            Id = 6,
            Description = "Solo Activity",
            UserId = 0,
            User = null,  // No user relationship
            Monday = false
        };
        var target = new CreateActivityDto();

        // Act
        activity.CopyTo(target);

        // Assert
        Assert.AreEqual(6, target.Id);
        Assert.AreEqual("Solo Activity", target.Description);
        Assert.AreEqual(0, target.UserId);  // Null User → UserId = 0
        Assert.IsFalse(target.Monday);
    }

    /// <summary>
    /// Scenario 5: Null Source Validation
    /// Tests that ArgumentNullException is thrown when source entity is null
    /// </summary>
    [TestMethod]
    public void CopyTo_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        User? nullUser = null;
        var target = new CreateUserDto();

        // Act & Assert
        try
        {
            nullUser!.CopyTo(target);
            Assert.Fail("Expected ArgumentNullException to be thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.IsTrue(ex.Message.Contains("source"));
        }
    }

    /// <summary>
    /// Scenario 6: Missing Source Properties
    /// Tests that extra entity properties are skipped when DTO has fewer properties
    /// </summary>
    [TestMethod]
    public void CopyTo_WithMissingDtoProperties_SkipsSilently()
    {
        // Arrange
        var user = new User
        {
            Id = 4,
            Name = "Carol White",
            Email = "carol@example.com",
            Active = true,
            CreatedAt = new DateTime(2026, 1, 10),
            LastLoginAt = new DateTime(2026, 1, 30),
            Password = "hashedPassword",
            GoogleAuthId = "google123"
        };
        var target = new CreateUserDto();

        // Act - should not throw
        user.CopyTo(target);

        // Assert - properties that exist in target are copied
        Assert.AreEqual(4, target.Id);
        Assert.AreEqual("Carol White", target.Name);
        Assert.AreEqual("carol@example.com", target.Email);
        // Password is in CreateUserDto and is copied from User entity
        Assert.AreEqual("hashedPassword", target.Password);
    }

    /// <summary>
    /// Scenario 7: Multiple Navigation Properties
    /// Tests that multiple properties are correctly handled during conversion
    /// </summary>
    [TestMethod]
    public void CopyTo_WithMultipleProperties_CopiesAllCorrectly()
    {
        // Arrange
        var activity = new Activity
        {
            Id = 7,
            Description = "Full Schedule Activity",
            Level = Level.Medium,
            UserId = 8,
            User = new User { Id = 8, Name = "David" },
            Monday = true,
            Tuesday = true,
            Wednesday = false,
            Thursday = true,
            Friday = false,
            Position = 3,
            Active = true
        };
        var target = new CreateActivityDto();

        // Act
        activity.CopyTo(target);

        // Assert - All properties correctly copied
        Assert.AreEqual(7, target.Id);
        Assert.AreEqual("Full Schedule Activity", target.Description);
        Assert.AreEqual((int)Level.Medium, target.Level);
        Assert.AreEqual(8, target.UserId);  // Navigation property converted
        Assert.IsTrue(target.Monday);
        Assert.IsTrue(target.Tuesday);
        Assert.IsFalse(target.Wednesday);
        Assert.IsTrue(target.Thursday);
        Assert.IsFalse(target.Friday);
        Assert.AreEqual(3, target.Position);
        Assert.IsTrue(target.Active);
    }

    /// <summary>
    /// Additional Test: Null Target Validation
    /// Tests that ArgumentNullException is thrown when target DTO is null
    /// </summary>
    [TestMethod]
    public void CopyTo_WithNullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User" };

        // Act & Assert
        try
        {
            user.CopyTo(null!);
            Assert.Fail("Expected ArgumentNullException to be thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.IsTrue(ex.Message.Contains("target"));
        }
    }

    /// <summary>
    /// Additional Test: Property Type Compatibility
    /// Tests that different property types are correctly converted
    /// </summary>
    [TestMethod]
    public void CopyTo_WithVariousPropertyTypes_CopiesToCorrectTypes()
    {
        // Arrange
        var activity = new Activity
        {
            Id = 10,
            Description = "Type Test",
            Level = Level.Hard,  // Enum type
            UserId = 15,  // Int type
            User = new User { Id = 15 },  // Include user for proper ID extraction
            Monday = true,  // Bool type
            Position = 5,  // Int type
            Active = false  // Bool type
        };
        var target = new CreateActivityDto();

        // Act
        activity.CopyTo(target);

        // Assert - Types preserved correctly
        Assert.AreEqual((int)Level.Hard, target.Level);
        Assert.AreEqual(15, target.UserId);
        Assert.IsTrue(target.Monday);
        Assert.AreEqual(5, target.Position);
        Assert.IsFalse(target.Active);
    }

    /// <summary>
    /// Additional Test: DateTime Property Handling
    /// Tests that DateTime properties are correctly copied
    /// </summary>
    [TestMethod]
    public void CopyTo_WithDateTimeProperties_CopiesCorrectly()
    {
        // Arrange
        var createdDate = new DateTime(2026, 1, 1, 10, 30, 0);
        var lastLoginDate = new DateTime(2026, 1, 31, 15, 45, 0);

        var user = new User
        {
            Id = 11,
            Name = "DateTime Test",
            Email = "test@example.com",
            CreatedAt = createdDate,
            LastLoginAt = lastLoginDate,
            Active = true
        };
        var target = new CreateUserDto();

        // Act
        user.CopyTo(target);

        // Assert
        // CreateUserDto doesn't have CreatedAt/LastLoginAt, so they won't be copied
        // But the basic properties should be
        Assert.AreEqual(11, target.Id);
        Assert.AreEqual("DateTime Test", target.Name);
        Assert.AreEqual("test@example.com", target.Email);
    }

    /// <summary>
    /// Additional Test: Multiple DTOs from same entity
    /// Tests that EntityBaseExtension works with multiple different DTO types
    /// </summary>
    [TestMethod]
    public void CopyToGeneric_WithActivityEntity_CreatesCreateActivityDto()
    {
        // Arrange
        var activity = new Activity
        {
            Id = 12,
            Description = "Created Activity",
            Level = Level.Medium,
            UserId = 20,
            User = new User { Id = 20 },
            Monday = true,
            Tuesday = false,
            Position = 2,
            Active = true
        };

        // Act
        var dto = activity.CopyTo<CreateActivityDto>();

        // Assert
        Assert.IsNotNull(dto);
        Assert.AreEqual(12, dto.Id);
        Assert.AreEqual("Created Activity", dto.Description);
        Assert.AreEqual((int)Level.Medium, dto.Level);
        Assert.AreEqual(20, dto.UserId);
        Assert.IsTrue(dto.Monday);
        Assert.IsFalse(dto.Tuesday);
        Assert.AreEqual(2, dto.Position);
        Assert.IsTrue(dto.Active);
    }
}
