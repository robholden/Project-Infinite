using Library.Core;
using Library.Service.PubSub;

using MassTransit;

using Reports.Core.Queries;
using Reports.Domain;

namespace Report.Api.Consumers.Reports;

public class ReportPictureConsumer : ISnowConsumer, IConsumer<ReportPictureRq>
{
    private readonly ReportContext _ctx;
    private readonly IReportQueries _queries;

    public ReportPictureConsumer(ReportContext ctx, IReportQueries queries)
    {
        _ctx = ctx;
        _queries = queries;
    }

    public async Task Consume(ConsumeContext<ReportPictureRq> context)
    {
        // Do not re-report
        var request = context.Message;
        var reported = await _queries.HasReportedPicture(request.User.UserId, request.PictureId);
        if (reported)
        {
            return;
        }

        // Retrieve existing report for user
        var report = await _ctx.PictureReports.FindOrNullAsync(x => x.PictureId == request.PictureId);
        var instance = new PictureReportInstance(request.User, request.Reason);
        if (report == null)
        {
            report = new(request.PictureUser, request.PictureId, request.PictureName, request.PicturePath)
            {
                Reports = new List<PictureReportInstance>() { instance }
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
