
using Library.Core;
using Library.Service.PubSub;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Moq;

using Reports.Core;
using Reports.Core.Queries;
using Reports.Core.Services;
using Reports.Domain;

namespace Reports.Tests;

internal class Helper
{
    public readonly UserRecord UserA;

    public readonly UserRecord UserB;

    public Helper()
    {
        Context = new ReportContext(
            new DbContextOptionsBuilder<ReportContext>()
                    .UseInMemoryDatabase("ReportDbTest")
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options
        );

        UserA = new(Guid.NewGuid(), "TesterA");
        UserB = new(Guid.NewGuid(), "TesterB");

        var mockIdentityEvents = new Mock<IIdentityPubSub>().Object;
        var mockContentEvents = new Mock<IContentPubSub>().Object;

        ReportQueries = new ReportQueries(Context);
        ReportService = new ReportService(Context, mockIdentityEvents, mockContentEvents);
    }

    public static ReportSettings ReportSettings => new();

    public ReportContext Context { get; }

    public IReportService ReportService { get; }

    public IReportQueries ReportQueries { get; }
}