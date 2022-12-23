
using Content.Core;
using Content.Domain;

using Library.Core;
using Library.Service.PubSub;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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

        // Create test base user
        User = new UserRecord(Guid.NewGuid(), "test");

        var mockCommEvents = new Mock<ICommsPubSub>().Object;
        var mockSocketEvents = new Mock<ISocketsPubSub>().Object;

    }

    public static ContentSettings ContentSettings => new()
    {
    };

    public ContentContext Context { get; }
}