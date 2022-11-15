using Comms.Core;
using Comms.Core.Queries;
using Comms.Core.Services;
using Comms.Domain;

using Library.Core;
using Library.Service.PubSub;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

namespace Comms.Tests;

internal class Helper
{
    public readonly UserSetting User;

    public Helper()
    {
        // Add database context
        Context = new CommsContext(
            new DbContextOptionsBuilder<CommsContext>()
                    .UseInMemoryDatabase("CommsDbTest")
                    .UseLazyLoadingProxies(false)
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options
        );

        // Create test base user
        User = AddUserData("TestUser");

        var emailOptions = Options.Create<EmailSettings>(new());
        var mockCommEvents = new Mock<ICommsPubSub>().Object;
        var mockSocketEvents = new Mock<ISocketsPubSub>().Object;

        NotificationQueries = new NotificationQueries(Context);
        NotificationService = new NotificationService(Context, emailOptions, mockCommEvents, mockSocketEvents);

        EmailService = new EmailService(Context, emailOptions);

        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(e => e.Send(It.IsAny<EmailQueue>()));
    }

    public CommsContext Context { get; }

    public IEmailService EmailService { get; }

    public INotificationService NotificationService { get; }

    public INotificationQueries NotificationQueries { get; }

    public static IConfigurationRoot GetIConfigurationRoot(string outputPath)
    {
        return new ConfigurationBuilder()
            .SetBasePath(outputPath)
            .AddJsonFile("appsettings.local.json", optional: true)
            .Build();
    }

    public static TextMagicSettings TextMagicSettings(string outputPath)
    {
        var configuration = new TextMagicSettings();
        var iConfig = GetIConfigurationRoot(outputPath);

        iConfig
            .GetSection("TextMagicSettings")
            .Bind(configuration);

        return configuration;
    }

    public UserRecord AddUser(string username)
    {
        var user = Context.UserSettings.Add(new() { UserId = Guid.NewGuid(), Email = "tester@example.com", Username = username, Name = "Tester" }).Entity;
        Context.SaveChanges();

        return new(user.UserId, username);
    }

    public UserSetting AddUserData(string name)
    {
        var user = Context.UserSettings.Add(new() { Email = "tester@example.com", Name = name }).Entity;
        Context.SaveChanges();

        return user;
    }

    public Notification AddTestNotification(string key = "0", Guid? userId = null)
    {
        var entity = new Notification()
        {
            UserId = userId ?? User.UserId,
            ContentKey = key,
            Type = NotificationType.None
        };

        entity = Context.Notifications.Add(entity).Entity;
        Context.SaveChanges();

        return entity;
    }
}