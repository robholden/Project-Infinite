
using Identity.Domain;

using Library.Core;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using OtpNet;

namespace Identity.Tests;

[TestClass]
public class TwoFactorTests
{
    private static readonly Helper helper = new();

    [TestMethod]
    public void Should_Generate_Code()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var secret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));

        // Act
        var outcome = helper.TwoFactorService.GenerateCode(userId, secret, TwoFactorType.App).TryRunTask(out var result, out var ex);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to generate code");
        Assert.IsFalse(string.IsNullOrEmpty(result), "Code is empty");
    }

    [TestMethod]
    public void Should_Verify_Code()
    {
        // Arrange
        var user = helper.AddTestUser();
        var type = TwoFactorType.App;
        var secret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));

        // Act
        var verified = false;
        var outcome = helper.TwoFactorService.GenerateCode(user.UserId, secret, TwoFactorType.App).TryRunTask(out var code, out var ex);
        if (outcome)
        {
            outcome = helper.TwoFactorService.VerifyCode(user.UserId, code, secret, type).TryRunTask(out verified, out ex);
        }

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to verify code");
        Assert.IsFalse(string.IsNullOrEmpty(code), "Code is empty");
        Assert.IsTrue(verified, "Failed to verify code");
    }

    [TestMethod]
    public void Should_Recover_Account()
    {
        // Arrange
        var testUser = helper.AddTestUser(new() { Name = "Test", Email = "test@test.com", Username = "RecoveryTest", EmailConfirmed = true }, true);
        var email = testUser.Email;
        var password = "Test123$";
        var identityKey = "unit_test";
        var ipAddress = "unit_test";
        var platform = "unit_test";
        var type = TwoFactorType.App;

        // Act
        var secret = helper.UserService.Setup2FA(testUser.UserId, type).Result;
        var code = helper.TwoFactorService.GenerateCode(testUser.UserId, secret, type).Result;
        helper.UserService.Enable2FA(testUser.UserId, code).Wait();
        var recoveryCodes = helper.TwoFactorService.GenerateRecoveryCodes(testUser.UserId).Result;
        var token = helper.AuthService.Login(email, password, new(identityKey, ipAddress, platform)).Result;
        helper.TwoFactorService.Recover(testUser.UserId, recoveryCodes.First().Value).Wait();
        var refreshToken = helper.AuthService.RefreshAuthToken(token.AuthTokenId, null, false).Result;

        // Assert
        Assert.IsTrue(refreshToken.TwoFactorPassed, "Two Factor has failed");
    }
}