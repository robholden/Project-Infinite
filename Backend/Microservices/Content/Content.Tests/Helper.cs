
using Content.Core;
using Content.Core.Services;
using Content.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
                    //.UseSqlServer("Server=(LocalDb)\\MSSQLLocalDB;Database=project_infinite_content;Trusted_Connection=True;MultipleActiveResultSets=true")
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options
        );

        // Create test base user
        User = new UserRecord(Guid.NewGuid(), "test");

        PostService = new PostService(Context);
    }

    public static ContentSettings ContentSettings => new()
    {
    };

    public ContentContext Context { get; }

    public IPostService PostService { get; set; }
}