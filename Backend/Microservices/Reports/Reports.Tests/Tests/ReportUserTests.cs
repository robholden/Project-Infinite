
using Library.Core;
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
public class ReportUserTests
{
    private static readonly Helper helper = new();

    [TestMethod]
    public void Should_Report_User()
    {
        // Arrange
        var user = helper.UserA;
        var reportedUser = new UserRecord(Guid.NewGuid(), "ReportedUser");
        var reportedUserName = "Reported User";
        var reportedUserEmail = "reported@user.com";
        var reason = ReportUserReason.Impersonation;

        var contextMoq = new Mock<ConsumeContext<ReportUserRq>>();
        contextMoq.Setup(c => c.Message).Returns(new ReportUserRq(user, reason, reportedUser, reportedUserName, reportedUserEmail));

        // Act
        var consumer = new ReportUserConsumer(helper.Context, helper.ReportQueries);
        var outcome = consumer.Consume(contextMoq.Object).TryRunTask(out var ex);
        var report = helper.Context.UserReports.Include(x => x.Reports).FirstOrDefault(x => x.UserId == reportedUser.UserId);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "An error occurred adding report");
        Assert.IsNotNull(report, "Failed to add report");
        Assert.AreEqual(1, report.Reports?.Count, "Report instances returned empty or null");
        Assert.AreEqual(user.UserId, report.Reports.First().UserId, "Report not assigned to user");
    }

    [TestMethod]
    public void Should_Get_Reported_Users()
    {
        // Arrange
        var pageRequest = new PagedListRequest<ReportQueryOptions>(1, 10);

        // Act
        AddReport();
        var outcome = helper.ReportQueries.Lookup<UserReport, UserReportInstance>(pageRequest).TryRunTask(out var result, out var ex);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to lookup reports");
        Assert.AreEqual(true, result?.Rows?.Any(), "Reports not found");
        Assert.AreEqual(true, result.Rows.First().Reports?.Any(), "Report instances returned empty or null");
    }

    [TestMethod]
    public void Should_Action_Reported_User()
    {
        // Arrange
        var report = AddReport();
        var user = new UserRecord(Guid.NewGuid(), "Moderator");
        var actionTaken = ReportedAction.Nothing;
        var notes = "Nothing";

        // Act
        var outcome = helper.ReportService.ActionUserReport(report.ReportId, actionTaken, user, notes, false).TryRunTask(out var action, out var ex);
        report = helper.Context.UserReports.Include(x => x.Action).FirstOrDefault(x => x.ReportId == report.ReportId);

        // Assert
        Assert.IsTrue(outcome, ex?.Message ?? "Failed to action report");
        Assert.IsNotNull(action, "Action returned as null");
        Assert.IsNotNull(report.Action, "Action not assigned to report");
    }

    private static UserReport AddReport(ReportUserReason reason = ReportUserReason.Impersonation)
    {
        var report = new UserReport(helper.UserA, "User A", "a@user.com")
        {
            Reports = new List<UserReportInstance>()
                {
                    new () { UserId = helper.UserB.UserId, Username = helper.UserB.Username, Reason = reason }
                }
        };

        helper.Context.UserReports.Add(report);
        helper.Context.SaveChanges();

        return report;
    }
}