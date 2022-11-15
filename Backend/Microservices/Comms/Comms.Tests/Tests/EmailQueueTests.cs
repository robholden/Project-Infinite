
using Comms.Api.Consumers.Emails;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Comms.Tests;

[TestClass]
public class EmailQueueTests
{
    private static readonly Helper helper = new();

    [TestMethod]
    public void Should_Add()
    {
        // Arrange
        var user = helper.AddUser("Tester");
        var message = "Test Content";
        var subject = "Unit Testing";
        var type = EmailType.Marketing;
        var hash = Guid.NewGuid().ToString();

        var contextMoq = new Mock<ConsumeContext<SendEmailToUserRq>>();
        contextMoq.Setup(c => c.Message).Returns(new SendEmailToUserRq(user, message, subject, type, hash));

        // Act
        var consumer = new SendEmailToUserConsumer(helper.Context, helper.EmailService);
        var outcome = consumer.Consume(contextMoq.Object).TryRunTask(out var ex);
        var emails = helper.Context.EmailQueue.Where(x => x.IdentityHash == hash).ToList();

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to add email to queue");
        Assert.AreEqual(1, emails.Count, "Failed to add email to queue");
    }
}