
using AutoMapper;

using Comms.Api.Consumers.Notifications;
using Comms.Core.Queries;

using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Comms.Tests;

[TestClass]
public class NotificationTests
{
    private static readonly Helper helper = new();

    [TestMethod]
    public void Should_Add()
    {
        // Arrange
        var user = new UserRecord(Guid.NewGuid(), "test");
        var identifier = Guid.NewGuid().ToString();
        var type = NotificationType.None;

        var contextMoq = new Mock<ConsumeContext<AddNotificationRq>>();
        contextMoq.Setup(c => c.Message).Returns(new AddNotificationRq(user, identifier, type, new()));

        // Act
        var consumer = new AddNotificationConsumer(new Mock<IMapper>().Object, helper.Context, helper.NotificationService);
        var outcome = consumer.Consume(contextMoq.Object).TryRunTask(out var ex);
        var notification = helper.Context.Notifications.FirstOrDefault(x => x.UserId == user.UserId);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to add notification");
        Assert.IsNotNull(notification, "Failed to add notification");
    }

    [TestMethod]
    public void Should_Add_Generic()
    {
        // Arrange
        var userLevel = UserLevel.Moderator;
        var identifier = Guid.NewGuid().ToString();
        var type = NotificationType.None;

        var contextMoq = new Mock<ConsumeContext<AddGeneralNotificationRq>>();
        contextMoq.Setup(c => c.Message).Returns(new AddGeneralNotificationRq(userLevel, identifier, type, new()));

        // Act
        var consumer = new AddGeneralNotificationConsumer(new Mock<IMapper>().Object, helper.Context, helper.NotificationService);
        var outcome = consumer.Consume(contextMoq.Object).TryRunTask(out var ex);
        var notification = helper.Context.Notifications.FirstOrDefault(x => x.UserLevel == userLevel && x.Identifier == identifier);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to add generic notification");
        Assert.IsNotNull(notification, "Failed to add generic notification");
    }

    [TestMethod]
    public void Should_Add_With_Entries()
    {
        // Arrange
        var user = new UserRecord(Guid.NewGuid(), "source");
        var triggeredUser = new UserRecord(Guid.NewGuid(), "trigger");
        var identifier = Guid.NewGuid().ToString();
        var type = NotificationType.None;

        var contextMoq = new Mock<ConsumeContext<AddNotificationRq>>();
        contextMoq.Setup(c => c.Message).Returns(new AddNotificationRq(user, identifier, type, new(), triggeredUser));

        // Act
        var consumer = new AddNotificationConsumer(new Mock<IMapper>().Object, helper.Context, helper.NotificationService);
        var outcome = consumer.Consume(contextMoq.Object).TryRunTask(out var ex);
        var notification = helper.Context.Notifications.Include(x => x.Entries).FirstOrDefault(x => x.UserId == user.UserId);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to add notification");
        Assert.IsNotNull(notification, "Failed to add notification");
        Assert.AreEqual(1, notification.Entries.Count, "Failed to add entries");
        Assert.AreEqual(triggeredUser.UserId, notification.Entries.First().UserId, "Entry user id does not match expected");
    }

    [TestMethod]
    public void Should_Read()
    {
        // Arrange
        var notification = helper.AddTestNotification();

        // Act
        var outcome = helper.NotificationService.MarkAsRead(notification.NotificationId).TryRunTask(out var ex);
        if (outcome)
        {
            notification = helper.NotificationQueries.Get(notification.NotificationId).Result;
        }

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to mark notification as read");
        Assert.IsTrue(notification.Read);
    }

    [TestMethod]
    public void Should_View()
    {
        // Arrange
        var notification = helper.AddTestNotification();

        // Act
        var outcome = helper.NotificationService.Viewed(notification.UserId).TryRunTask(out var ex);
        if (outcome)
        {
            notification = helper.NotificationQueries.Get(notification.NotificationId).Result;
        }

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to view notification for user");
        Assert.IsTrue(notification.Viewed, "Failed to view notification");
    }

    [TestMethod]
    public void Should_Get_By_User()
    {
        // Arrange
        var userId = Guid.NewGuid();
        helper.AddTestNotification("1", userId);
        helper.AddTestNotification("2", userId);
        helper.AddTestNotification("3", userId);

        var options = new NotificationQueryOptions { UserId = userId };
        var request = new PagedListRequest<NotificationQueryOptions>(1, 10, OrderByDirection.Desc, options);

        // Act
        var outcome = helper.NotificationQueries.Lookup(request).TryRunTask(out var result, out var ex);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to add notification");
        Assert.AreEqual(result.TotalRows, 3, $"Total rows value is incorrect, expected 3 got {result.TotalRows}");
        Assert.AreEqual(result.Rows?.Count(), 3, $"Rows data is incorrect, expected 3 got {result.Rows?.Count()}");
    }
}