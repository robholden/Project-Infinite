
using Library.Core;
using Library.Core.Enums;
using Library.Core.Models;
using Library.Service.PubSub;

using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Report.Api.Consumers.Reports;

using Reports.Core.Queries;
using Reports.Domain;

namespace Reports.Tests;

[TestClass]
public class ReportPictureTests
{
    private static readonly Helper helper = new();

    [TestMethod]
    public void Should_Report_Picture()
    {
        // Arrange
        var user = helper.UserA;
        var reportedUser = helper.UserB;
        var reportedPictureId = Guid.NewGuid();
        var reportedPictureName = "Reported Picture";
        var reportedPicturePath = "Path.jpg";
        var reason = ReportPictureReason.Copyright;

        var contextMoq = new Mock<ConsumeContext<ReportPictureRq>>();
        contextMoq.Setup(c => c.Message).Returns(new ReportPictureRq(user, reason, reportedPictureId, reportedUser, reportedPictureName, reportedPicturePath));

        // Act
        var consumer = new ReportPictureConsumer(helper.Context, helper.ReportQueries);
        var outcome = consumer.Consume(contextMoq.Object).TryRunTask(out var ex);
        var report = helper.Context.PictureReports.Include(x => x.Reports).FirstOrDefault(x => x.PictureId == reportedPictureId);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "An error occurred adding report");
        Assert.IsNotNull(report, "Failed to add report");
        Assert.AreEqual(1, report.Reports?.Count, "Report instances returned empty or null");
        Assert.AreEqual(user.UserId, report.Reports.First().UserId, "Report not assigned to user");
    }

    [TestMethod]
    public void Should_Get_Reported_Pictures()
    {
        // Arrange
        var pageRequest = new PagedListRequest<ReportQueryOptions>(1, 10);

        // Act
        AddReport();
        var outcome = helper.ReportQueries.Lookup<PictureReport, PictureReportInstance>(pageRequest).TryRunTask(out var result, out var ex);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to lookup reports");
        Assert.AreEqual(true, result?.Rows?.Any(), "Reports not found");
        Assert.AreEqual(true, result.Rows.First().Reports?.Any(), "Report instances returned empty or null");
    }

    [TestMethod]
    public void Should_Action_Reported_Picture()
    {
        // Arrange
        var report = AddReport();
        var user = new UserRecord(Guid.NewGuid(), "Moderator");
        var actionTaken = ReportedAction.Nothing;
        var notes = "Nothing";

        // Act
        var outcome = helper.ReportService.ActionPictureReport(report.ReportId, actionTaken, user, notes, false).TryRunTask(out var action, out var ex);
        report = helper.Context.PictureReports.Include(x => x.Action).FirstOrDefault(x => x.ReportId == report.ReportId);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to action report");
        Assert.IsNotNull(action, "Action returned as null");
        Assert.IsNotNull(report.Action, "Action not assigned to report");
    }

    private static PictureReport AddReport(ReportPictureReason reason = ReportPictureReason.Copyright)
    {
        var report = new PictureReport(helper.UserA, Guid.NewGuid(), "Picture Name", "path.jpg")
        {
            Reports = new List<PictureReportInstance>()
                {
                    new () { UserId = helper.UserB.UserId, Username = helper.UserB.Username, Reason = reason }
                }
        };

        helper.Context.PictureReports.Add(report);
        helper.Context.SaveChanges();

        return report;
    }
}