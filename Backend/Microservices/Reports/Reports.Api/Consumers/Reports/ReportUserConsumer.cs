using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Reports.Core.Queries;
using Reports.Domain;

namespace Report.Api.Consumers.Reports;

public class ReportUserConsumer : IRabbitConsumer, IConsumer<ReportUserRq>
{
    private readonly ReportContext _ctx;
    private readonly IReportQueries _queries;

    public ReportUserConsumer(ReportContext ctx, IReportQueries queries)
    {
        _ctx = ctx;
        _queries = queries;
    }

    public async Task Consume(ConsumeContext<ReportUserRq> context)
    {
        // Do not re-report
        var request = context.Message;
        var reported = await _queries.HasReportedUser(request.User.UserId, request.ReportedUser.UserId);
        if (reported)
        {
            return;
        }

        // Retrieve existing report for user
        var report = await _ctx.UserReports.FindOrNullAsync(x => x.UserId == request.ReportedUser.UserId);
        var instance = new UserReportInstance(request.User, request.Reason);
        if (report == null)
        {
            report = new(request.ReportedUser, request.Name, request.Email)
            {
                Reports = new List<UserReportInstance>() { instance }
            };
            report = await _ctx.CreateAsync(report);
        }
        else
        {
            report.Date = DateTime.UtcNow;
            report.Reports.Add(instance);
            report = await _ctx.UpdateAsync(report);
        }
    }
}
