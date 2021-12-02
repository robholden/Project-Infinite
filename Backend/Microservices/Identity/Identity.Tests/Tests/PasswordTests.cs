
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Identity.Tests;

[TestClass]
public class PasswordTests
{
    private static readonly Helper identity = new();

    [TestMethod]
    public void Should_Change()
    {
        // Arrange
        var user = identity.AddTestUser();
        var oldPassword = "Test123$";
        var newPassword = "Test4321$";

        // Act
        identity.PasswordService.Change(user.UserId, oldPassword, newPassword);

        // Assert
        var pw = identity.Context.Passwords.FirstOrDefault(p => p.UserId == user.UserId);
        Assert.IsNotNull(pw, "Failed to add password");
        Assert.IsTrue(pw.Verify(newPassword), "Failed to match passwords");
    }

    [TestMethod]
    public void Should_Set()
    {
        // Arrange
        var user = identity.AddTestUser(false);
        var password = "Test123$";

        // Act
        identity.PasswordService.Set(user.UserId, password);

        // Assert
        var pw = identity.Context.Passwords.FirstOrDefault(p => p.UserId == user.UserId);
        Assert.IsNotNull(pw, "Failed to add password");
        Assert.IsTrue(pw.Verify(password), "Failed to match passwords");
    }
}