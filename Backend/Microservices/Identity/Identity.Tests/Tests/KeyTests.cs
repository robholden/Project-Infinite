using Identity.Domain;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Identity.Tests;

[TestClass]
public class KeyTests
{
    private static readonly Helper identity = new();

    [TestMethod]
    public void Should_Create()
    {
        // Arrange
        var user = identity.AddTestUser();
        var type = UserKeyType.ConfirmEmail;

        // Act
        var key = identity.UserKeyService.Create(user.UserId, type, null).Result;

        // Assert
        Assert.IsNotNull(key, "Failed to add key");
        Assert.AreEqual(key.Type, type, "Failed to set key type");
    }

    [TestMethod]
    public void Should_UseKey()
    {
        // Arrange
        var user = identity.AddTestUser();

        // Act
        var key = identity.UserKeyService.Create(user.UserId, UserKeyType.ConfirmEmail, null).Result;
        key = identity.UserKeyService.UseKey(key.Key, key.Type).Result;

        // Assert
        Assert.IsNotNull(key, "Failed to get key");
        Assert.IsNotNull(key.UsedAt, "Failed to set use at value");
    }
}