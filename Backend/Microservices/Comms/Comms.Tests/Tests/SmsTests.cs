using Comms.Core.SMS;

using Library.Core;

using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Comms.Tests;

[TestClass]
public class SmsTests
{
    public TestContext TestContext { get; set; }

    [Ignore]
    [TestMethod]
    public void Should_Send_SMS_TextMagic()
    {
        // Arrange
        var settings = Helper.TextMagicSettings(TestContext.DeploymentDirectory);
        if (settings == null) return;

        var number = "+447882466772";
        var message = "This is a text message :)";

        // Act
        var client = new TextMagicSmsProvider(Options.Create(settings));
        var outcome = client.SendAsync(number, message).TryRunTask(out var result, out var exc);

        // Assert
        Assert.IsTrue(outcome, exc?.Message ?? "Failed to send sms");
        Assert.IsTrue(result.sent, $"Failed to send SMS: {result.error}");
    }
}