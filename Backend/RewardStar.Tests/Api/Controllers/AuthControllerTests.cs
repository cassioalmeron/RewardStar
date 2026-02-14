using RewardStar.Api.Controllers;
using RewardStar.Api.DTOs;
using RewardStar.Core.Models;
using System.Reflection;

namespace RewardStar.Tests.Api.Controllers;

[TestClass]
public class AuthControllerTests
{
    /// <summary>
    /// Helper method to invoke the private MapToAuthResponseDto method via reflection
    /// </summary>
    private AuthResponseDto InvokeMapToAuthResponseDto(User user, string token)
    {
        var method = typeof(AuthController).GetMethod("MapToAuthResponseDto",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            new[] { typeof(User), typeof(string) },
            null);

        return (AuthResponseDto)method?.Invoke(null, new object[] { user, token })!;
    }

    /// <summary>
    /// Test that MapToAuthResponseDto correctly copies all User properties to AuthResponseDto
    /// Invokes the actual private helper method in AuthController
    /// </summary>
    [TestMethod]
    public void MapToAuthResponseDto_WithValidUser_CopiesAllProperties()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com",
            Active = true
        };
        var token = "jwt.token.here";

        // Act - Invoke the actual private MapToAuthResponseDto method from AuthController
        var dto = InvokeMapToAuthResponseDto(user, token);

        // Assert
        Assert.AreEqual(1, dto.UserId);
        Assert.AreEqual("John Doe", dto.Name);
        Assert.AreEqual("john@example.com", dto.Email);
        Assert.AreEqual(token, dto.Token);
        Assert.IsTrue(dto.Active);
    }

    /// <summary>
    /// Test that MapToAuthResponseDto includes GoogleAuthId when present
    /// </summary>
    [TestMethod]
    public void MapToAuthResponseDto_WithGoogleAccount_IncludesGoogleAuthId()
    {
        // Arrange
        var user = new User
        {
            Id = 2,
            Name = "Jane Smith",
            Email = "jane@example.com",
            GoogleAuthId = "google123",
            Active = true
        };
        var token = "jwt.token.google";

        // Act
        var dto = InvokeMapToAuthResponseDto(user, token);

        // Assert
        Assert.AreEqual(2, dto.UserId);
        Assert.AreEqual("Jane Smith", dto.Name);
        Assert.AreEqual("jane@example.com", dto.Email);
        Assert.AreEqual(token, dto.Token);
        Assert.AreEqual("google123", dto.GoogleAuthId);
        Assert.IsTrue(dto.Active);
    }

    /// <summary>
    /// Test that MapToAuthResponseDto preserves User ID mapping to UserId in DTO
    /// </summary>
    [TestMethod]
    public void MapToAuthResponseDto_WithMultipleUsers_CorrectlyMapsUserId()
    {
        // Arrange
        var user1 = new User { Id = 10, Name = "User 1", Email = "user1@example.com", Active = true };
        var user2 = new User { Id = 20, Name = "User 2", Email = "user2@example.com", Active = true };

        // Act
        var dto1 = InvokeMapToAuthResponseDto(user1, "token1");
        var dto2 = InvokeMapToAuthResponseDto(user2, "token2");

        // Assert
        Assert.AreEqual(10, dto1.UserId);
        Assert.AreEqual(20, dto2.UserId);
    }

    /// <summary>
    /// Test that AuthResponseDto correctly handles user without Google account
    /// </summary>
    [TestMethod]
    public void MapToAuthResponseDto_WithoutGoogleAuth_GoogleAuthIdIsNull()
    {
        // Arrange
        var user = new User
        {
            Id = 3,
            Name = "Local User",
            Email = "local@example.com",
            GoogleAuthId = null,
            Active = true
        };
        var token = "jwt.token.local";

        // Act
        var dto = InvokeMapToAuthResponseDto(user, token);

        // Assert
        Assert.IsNull(dto.GoogleAuthId);
        Assert.AreEqual("Local User", dto.Name);
        Assert.AreEqual(token, dto.Token);
    }

    /// <summary>
    /// Test that AuthResponseDto correctly handles inactive users
    /// </summary>
    [TestMethod]
    public void MapToAuthResponseDto_WithInactiveUser_PreservesActiveFlag()
    {
        // Arrange
        var user = new User
        {
            Id = 4,
            Name = "Inactive User",
            Email = "inactive@example.com",
            Active = false
        };
        var token = "jwt.token.inactive";

        // Act
        var dto = InvokeMapToAuthResponseDto(user, token);

        // Assert
        Assert.IsFalse(dto.Active);
        Assert.AreEqual("Inactive User", dto.Name);
        Assert.AreEqual(token, dto.Token);
    }

    /// <summary>
    /// Test that multiple users can be mapped to AuthResponseDto correctly
    /// </summary>
    [TestMethod]
    public void MapToAuthResponseDto_WithMultipleUsers_AllPropertiesCorrectlyMapped()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Name = "Alice", Email = "alice@example.com", Active = true },
            new() { Id = 2, Name = "Bob", Email = "bob@example.com", GoogleAuthId = "google_bob", Active = true },
            new() { Id = 3, Name = "Charlie", Email = "charlie@example.com", Active = false }
        };

        // Act
        var dtos = users.Select(u => InvokeMapToAuthResponseDto(u, $"token_{u.Id}")).ToList();

        // Assert
        Assert.AreEqual(3, dtos.Count);

        Assert.AreEqual(1, dtos[0].UserId);
        Assert.AreEqual("Alice", dtos[0].Name);
        Assert.IsNull(dtos[0].GoogleAuthId);
        Assert.IsTrue(dtos[0].Active);

        Assert.AreEqual(2, dtos[1].UserId);
        Assert.AreEqual("Bob", dtos[1].Name);
        Assert.AreEqual("google_bob", dtos[1].GoogleAuthId);
        Assert.IsTrue(dtos[1].Active);

        Assert.AreEqual(3, dtos[2].UserId);
        Assert.AreEqual("Charlie", dtos[2].Name);
        Assert.IsFalse(dtos[2].Active);
    }
}
