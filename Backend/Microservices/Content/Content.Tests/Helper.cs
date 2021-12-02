
using Content.Core;
using Content.Core.Queries;
using Content.Core.Services;
using Content.Domain;

using Library.Core.Models;
using Library.Service.PubSub;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using Moq;

namespace Content.Tests;

internal class Helper
{
    public readonly IUser User;

    public Helper()
    {
        // Add database context
        Context = new ContentContext(
            new DbContextOptionsBuilder<ContentContext>()
                    .UseInMemoryDatabase("ContentDbTest")
                    .UseLazyLoadingProxies(false)
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options
        );
        Context.Seed();

        // Create test base user
        User = AddUser("TestUser");

        var mockCommEvents = new Mock<ICommsPubSub>().Object;
        var mockSocketEvents = new Mock<ISocketsPubSub>().Object;

        // Create services
        LocationQueries = new LocationQueries(Context);
        LocationService = new LocationService(Context, Options.Create(ContentSettings), mockSocketEvents);

        CollectionService = new CollectionService(Context);
        CollectionQueries = new CollectionQueries(Context);

        PictureService = new PictureService(Context, Options.Create(PictureSettings), LocationQueries, mockCommEvents, mockSocketEvents);
        PictureQueries = new PictureQueries(Context, new Mock<IMemoryCache>().Object);
    }

    public static ContentSettings ContentSettings => new()
    {
        LocationIQToken = "ae479581586c2f"
    };

    public static PictureSettings PictureSettings => new();

    public ContentContext Context { get; }

    public ILocationService LocationService { get; }

    public ILocationQueries LocationQueries { get; }

    public IPictureService PictureService { get; }

    public IPictureQueries PictureQueries { get; }

    public ICollectionService CollectionService { get; }

    public ICollectionQueries CollectionQueries { get; }

    public IUser AddUser(string username)
    {
        var user = new UserRecord(Guid.NewGuid(), username);

        Context.UserSettings.Add(new(user.UserId));
        Context.SaveChanges();

        return user;
    }

    public Picture AddTestPicture()
    {
        var index = Context.Pictures.Count() + 1;
        var picture = new Picture()
        {
            Name = "Unit Test " + index,
            IpAddress = "unit_test",
            DateTaken = DateTime.Now,
            Format = ".unit_test",
            UserId = User.UserId,
            Username = User.Username,
            Location = new()
            {
                Boundry = new(),
                Country = Context.Countries.First(),
                Name = "Test"
            },
            Hash = "#",
            Width = 0,
            Height = 0,
            Ext = "#",
            Status = PictureStatus.Published,
            Colours = "blue,red,green,purple"
        };

        Context.Pictures.Add(picture);
        Context.SaveChanges();
        return picture;
    }
}