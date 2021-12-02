
using Identity.Core.Services;
using Identity.Domain;

using Library.Core;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Identity.Tests;

[TestClass]
public class UserTests
{
    private static readonly Helper identity = new();

    [TestMethod]
    public void Should_Confirm_Email()
    {
        // Arrange
        var testUser = identity.AddTestUser();

        // Act
        var key = identity.UserService.SendEmailConfirmation(testUser.UserId, false).Result;
        identity.UserService.VerifyAndConfirmEmail(key).Wait();

        var user = identity.UserQueries.Get(testUser.UserId).Result;

        // Assert
        Assert.IsTrue(user.EmailConfirmed, "Failed confirmed is not true");
    }

    [TestMethod]
    public void Should_Enable2FA()
    {
        // Arrange
        var type = TwoFactorType.Email;
        var user = identity.BaseUserModel();
        user.EmailConfirmed = true;
        identity.AddTestUser(user);

        // Act
        var secret = identity.UserService.Setup2FA(user.UserId, type).Result;
        var code = identity.TwoFactorService.GenerateCode(user.UserId, secret, type).Result;
        identity.UserService.Enable2FA(user.UserId, code);
        user = identity.UserQueries.Get(user.UserId).Result;

        // Assert
        Assert.AreEqual(type, user.TwoFactorType, "Failed to set type");
        Assert.AreEqual(true, user.TwoFactorEnabled, "Failed to set type");
    }

    [TestMethod]
    public void Should_Fail_To_Register_DuplicateEmail()
    {
        // Arrange
        var testUser = identity.AddTestUser();
        var register = new RegisterRequest("Tester1", "Testing", testUser.Email, "Test123$", false);

        // Act
        var ex = Assert.ThrowsExceptionAsync<SiteException>(() => identity.UserService.Register(register)).Result;

        // Assert
        Assert.AreEqual(ErrorCode.EmailInUse, ex.ErrorCode);
    }

    [TestMethod]
    public void Should_Get_By_Email()
    {
        // Arrange
        var testUser = identity.AddTestUser();

        // Act
        var user = identity.UserQueries.GetByEmail(testUser.Email).Result;

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual(testUser.Email, user.Email);
    }

    [TestMethod]
    public void Should_Register()
    {
        // Arrange
        var register = new RegisterRequest("Tester2", "Testing", "register@test.com", "Test123$", false);

        // Act
        var completed = identity.UserService.Register(register).TryRunTask(out var user, out var exc);

        // Assert
        Assert.IsTrue(completed, exc?.Message ?? "Failed to run register user");
        Assert.IsNotNull(user, "Failed to register user");
        Assert.IsNotNull(identity.Context.Users.FirstOrDefault(u => u.UserId == user.UserId), "Failed to add user to context");
        Assert.IsNotNull(identity.Context.Passwords.FirstOrDefault(p => p.UserId == user.UserId), "Failed to add password to context");
    }

    [TestMethod]
    public void Should_Send_Email_And_Confirm()
    {
        // Arrange
        var testUser = identity.AddTestUser();
        identity.Context.UserKeys.Add(new UserKey(testUser.UserId, UserKeyType.ConfirmEmail, null));
        identity.Context.SaveChanges();

        // Act
        var key = identity.UserService.SendEmailConfirmation(testUser.UserId, false).Result;
        var user = identity.UserQueries.Get(testUser.UserId).Result;

        // Assert
        Assert.IsNotNull(key, "User key has not been created");
        Assert.IsFalse(user.EmailConfirmed, "Emailed confirmed is not false");

        // Act again...
        identity.UserService.VerifyAndConfirmEmail(key).Wait();
        user = identity.UserQueries.Get(testUser.UserId).Result;

        // Assert
        Assert.IsTrue(user.EmailConfirmed, "Email has not been confirmed");
    }
}