using Library.Core;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Identity.Tests;

[TestClass]
public class AuthTests
{
    private static readonly Helper helper = new();

    [TestMethod]
    public void Should_Login()
    {
        // Arrange
        var testUser = helper.AddTestUser();
        var email = testUser.Email;
        var password = "Test123$";
        var identityKey = "unit_test";
        var ipAddress = "unit_test";
        var platform = "unit_test";

        // Act
        var token = helper.AuthService.Login(email, password, new(identityKey, ipAddress, platform)).Result;

        // Assert
        Assert.IsNotNull(token, "Token not created");
        Assert.IsNotNull(token.User, "User failed to logged in");
    }

    [TestMethod]
    public void Should_Get_Account_Locked()
    {
        // Arrange
        var testUser = helper.AddTestUser();
        var email = testUser.Email;
        var password = "wrong";
        var identityKey = "unit_test";
        var ipAddress = "unit_test_2";
        var platform = "unit_test";

        // Act
        for (var i = 0; i < Helper.Settings.FailedLoginAttempts; i++)
        {
            try { helper.AuthService.Login(email, password, new(identityKey, ipAddress, platform)).Wait(); }
            catch { }
        }

        var ex = Assert.ThrowsExceptionAsync<SiteException>(() => helper.AuthService.Login(email, password, new(identityKey, ipAddress, platform))).Result;

        // Assert
        Assert.AreEqual(ErrorCode.AccountLocked, ex.ErrorCode);
    }
}