﻿using Library.Core;
using Library.Service.PubSub;

using Reports.Domain;

namespace Reports.Core.Services;

public class ReportService : IReportService
{
    private readonly ReportContext _ctx;

    private readonly IIdentityPubSub _identityEvents;
    private readonly IContentPubSub _contentEvents;

    public ReportService(ReportContext ctx, IIdentityPubSub identityEvents, IContentPubSub contentEvents)
    {
        _ctx = ctx;

        _identityEvents = identityEvents;
        _contentEvents = contentEvents;
    }

    public async Task<ReportAction> ActionUserReport(Guid reportId, ReportedAction actionTaken, IUser actionedBy, string notes, bool sendEmail)
    {
        // Get report
        var report = await _ctx.UserReports.FindOrNullAsync(x => x.ReportId == reportId);
        if (report?.ActionId.HasValue != false)
        {
            return null;
        }

        // Add action to report
        report.Action = new ReportAction(actionedBy, actionTaken, notes);
        report = await _ctx.UpdateAsync(report);

        // Call identity service and delete user
        if (actionTaken == ReportedAction.Deleted)
        {
            await _identityEvents.DeleteReportedUser(new(report.UserId, sendEmail));
        }

        return report.Action;
    }
}