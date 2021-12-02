
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Identity.Tests;

[TestClass]
public class RecoveryCodeTests
{
    private static readonly Helper identity = new();

    [TestMethod]
    public void Should_Generate()
    {
        // Arrange
        var testUser = identity.AddTestUser();

        // Act
        var codes = identity.TwoFactorService.GenerateRecoveryCodes(testUser.UserId).Result;

        // Assert
        Assert.IsTrue(codes.Any(), "Failed to add codes");
    }
}