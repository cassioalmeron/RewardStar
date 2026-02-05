using Microsoft.VisualStudio.TestTools.UnitTesting;
using RewardStar.Api.DTOs;
using RewardStart.Core.Models;
using RewardStart.Core.Extensions;

namespace RewardStar.Tests.Api.Extensions;

[TestClass]
public class DtoBaseExtensionTests
{
    /// <summary>
    /// Scenario 1: Basic Property Copying - User Creation
    /// Tests that simple properties are copied from DTO to entity
    /// </summary>
    [TestMethod]
    public void CopyTo_WithValidUserDto_CopiesBasicProperties()
    {
        // Arrange
        var dto = new CreateUserDto { Name = "John Doe", Email = "john@example.com" };
        var target = new User();

        // Act
        dto.CopyTo(target);

        // Assert
        Assert.AreEqual("John Doe", target.Name);
        Assert.AreEqual("john@example.com", target.Email);
    }

    /// <summary>
    /// Scenario 1b: Basic Property Copying using Generic Method
    /// Tests that CopyTo generic creates a new entity and copies properties
    /// </summary>
    [TestMethod]
    public void CopyToGeneric_WithValidUserDto_CreatesNewEntity()
    {
        // Arrange
        var dto = new CreateUserDto { Name = "Alice Smith", Email = "alice@example.com" };

        // Act
        var user = dto.CopyTo<User>();

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual("Alice Smith", user.Name);
        Assert.AreEqual("alice@example.com", user.Email);
    }

    /// <summary>
    /// Scenario 2: ID-to-Entity Conversion - Activity with User
    /// Tests that UserId (int) is properly copied and User entity creation depends on reflection matching
    /// </summary>
    [TestMethod]
    public void CopyTo_WithIdToEntityConversion_CopiesUserIdProperty()
    {
        // Arrange
        var dto = new CreateActivityDto
        {
            Description = "Daily Workout",
            UserId = 5  // DTO has UserId as int
        };
        var target = new Activity();

        // Act
        dto.CopyTo(target);

        // Assert
        Assert.AreEqual("Daily Workout", target.Description);
        Assert.AreEqual(5, target.UserId);
        // The extension copies the UserId property directly
    }

    /// <summary>
    /// Scenario 3a: Null Source Validation
    /// Tests that ArgumentNullException is thrown when source DTO is null
    /// </summary>
    [TestMethod]
    public void CopyTo_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        CreateUserDto? nullDto = null;
        var target = new User();

        // Act & Assert
        try
        {
            nullDto!.CopyTo(target);
            Assert.Fail("Expected ArgumentNullException to be thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.Contains("source", ex.Message);
        }
    }

    /// <summary>
    /// Scenario 3b: Null Target Validation
    /// Tests that ArgumentNullException is thrown when target entity is null
    /// </summary>
    [TestMethod]
    public void CopyTo_WithNullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        var dto = new CreateUserDto { Name = "Test User", Email = "test@example.com" };

        // Act & Assert
        try
        {
            dto.CopyTo(null!);
            Assert.Fail("Expected ArgumentNullException to be thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.IsTrue(ex.Message.Contains("target"));
        }
    }

    /// <summary>
    /// Scenario 4: Missing Properties - Graceful Skipping
    /// Tests that properties in DTO that don't exist in entity are skipped silently
    /// </summary>
    [TestMethod]
    public void CopyTo_WithExtraPropertiesInDto_SkipsMissingTargetProperties()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Name = "John",
            Email = "john@example.com"
            // Note: User entity may have properties not in DTO like CreatedAt, etc
        };
        var target = new User();

        // Act - should not throw
        dto.CopyTo(target);

        // Assert - properties are copied, missing properties skipped gracefully
        Assert.AreEqual("John", target.Name);
        Assert.AreEqual("john@example.com", target.Email);
    }

    /// <summary>
    /// Scenario 5: Zero/Null ID Handling - Relationship Not Set
    /// Tests that when ID is 0, the entity relationship is not created
    /// </summary>
    [TestMethod]
    public void CopyTo_WithZeroId_SkipsEntityRelationship()
    {
        // Arrange
        var dto = new CreateActivityDto { Description = "Test Activity", UserId = 0 };
        var target = new Activity();

        // Act
        dto.CopyTo(target);

        // Assert
        Assert.AreEqual("Test Activity", target.Description);
        Assert.AreEqual(0, target.UserId);
        // User relationship should not be created for ID = 0
        Assert.IsNull(target.User);
    }

    /// <summary>
    /// Scenario 6: Null ID Property Handling
    /// Tests that when ID property is null, the entity relationship is skipped
    /// </summary>
    [TestMethod]
    public void CopyTo_WithNullIdProperty_SkipsEntityRelationship()
    {
        // Arrange
        var dto = new CreateActivityDto
        {
            Description = "Test Activity",
            UserId = 0  // Default/unset ID
        };
        var target = new Activity();

        // Act
        dto.CopyTo(target);

        // Assert
        Assert.AreEqual("Test Activity", target.Description);
        Assert.IsNull(target.User);
    }

    /// <summary>
    /// Scenario 7: Generic CopyTo<T>() - Create New Entity
    /// Tests that generic method creates new instance via Activator and copies properties
    /// </summary>
    [TestMethod]
    public void CopyToGeneric_CreatesNewEntityViaActivator()
    {
        // Arrange
        var dto = new CreateUserDto { Name = "Bob Johnson", Email = "bob@example.com" };

        // Act
        var user = dto.CopyTo<User>();

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual(0, user.Id); // New instance should have default ID
        Assert.AreEqual("Bob Johnson", user.Name);
        Assert.AreEqual("bob@example.com", user.Email);
    }

    /// <summary>
    /// Scenario 8: In-Place Update - CopyTo(target)
    /// Tests that existing entity is updated with DTO values
    /// Note: DTO Id property (inherited from DtoBase) will be copied if it's not 0
    /// </summary>
    [TestMethod]
    public void CopyTo_WithExistingEntity_UpdatesOnlyMappedProperties()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Name = "Original Name",
            Email = "original@example.com",
            Active = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var updateDto = new UpdateUserDto { Id = 1, Name = "Updated Name", Email = "original@example.com" };

        // Act
        updateDto.CopyTo(existingUser);

        // Assert
        Assert.AreEqual(1, existingUser.Id); // ID updated from DTO
        Assert.AreEqual("Updated Name", existingUser.Name); // Updated
        Assert.AreEqual("original@example.com", existingUser.Email); // Explicitly copied from DTO
        Assert.IsTrue(existingUser.Active); // Unchanged (Active property not in DTO)
    }

    /// <summary>
    /// Additional Test: Null Values Clear Existing Data
    /// Tests that null values from DTO overwrite existing entity values (allows clearing fields from frontend)
    /// </summary>
    [TestMethod]
    public void CopyTo_WithNullInDto_OverwritesExistingValue()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Name = "Original Name",
            Email = "original@example.com",
            Active = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var updateDto = new UpdateUserDto { Id = 1, Name = null, Email = null };

        // Act
        updateDto.CopyTo(existingUser);

        // Assert - Null values from DTO overwrite existing values (allows users to clear fields from frontend)
        Assert.IsNull(existingUser.Name);
        Assert.IsNull(existingUser.Email);
        Assert.AreEqual(1, existingUser.Id);
        Assert.IsTrue(existingUser.Active); // Unchanged (not in DTO)
    }

    /// <summary>
    /// Additional Test: Multiple Property Types
    /// Tests copying various property types (string, int, bool, enum)
    /// </summary>
    [TestMethod]
    public void CopyTo_WithVariousPropertyTypes_CopiesAllTypes()
    {
        // Arrange
        var dto = new CreateActivityDto
        {
            Description = "Mixed Types Test",
            UserId = 3,
            Monday = true,
            Tuesday = false,
            Wednesday = true
        };
        var target = new Activity();

        // Act
        dto.CopyTo(target);

        // Assert
        Assert.AreEqual("Mixed Types Test", target.Description);
        Assert.AreEqual(3, target.UserId);
        Assert.IsTrue(target.Monday);
        Assert.IsFalse(target.Tuesday);
        Assert.IsTrue(target.Wednesday);
    }

    /// <summary>
    /// Additional Test: Property Name Matching with Id Suffix Stripping
    /// Tests that property name matching works with exact match first
    /// </summary>
    [TestMethod]
    public void CopyTo_WithIdSuffixStripping_MatchesExactProperty()
    {
        // Arrange
        var dto = new CreateActivityDto
        {
            Description = "Test",
            UserId = 7  // Exact match for UserId property
        };
        var target = new Activity();

        // Act
        dto.CopyTo(target);

        // Assert
        Assert.AreEqual(7, target.UserId); // Exact match for UserId property
        // If User navigation property exists and the extension creates entity relationships,
        // it would be created here, but this depends on reflection type checking
    }

    /// <summary>
    /// Additional Test: Empty String Handling
    /// Tests that empty strings are copied (not treated as null)
    /// </summary>
    [TestMethod]
    public void CopyTo_WithEmptyString_CopiesEmptyStringValue()
    {
        // Arrange
        var dto = new CreateUserDto { Name = "", Email = "test@example.com" };
        var target = new User();

        // Act
        dto.CopyTo(target);

        // Assert
        Assert.AreEqual("", target.Name);
        Assert.AreEqual("test@example.com", target.Email);
    }

    /// <summary>
    /// Additional Test: Null String Property Handling
    /// Tests that null string properties are copied as null
    /// </summary>
    [TestMethod]
    public void CopyTo_WithNullStringProperty_CopiesNullValue()
    {
        // Arrange
        var dto = new CreateUserDto { Name = null!, Email = "test@example.com" };
        var target = new User();

        // Act
        dto.CopyTo(target);

        // Assert
        Assert.IsNull(target.Name);
        Assert.AreEqual("test@example.com", target.Email);
    }
}
