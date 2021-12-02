using System.Linq.Expressions;

using Library.Core;

using Microsoft.EntityFrameworkCore;

using Reports.Domain;

namespace Reports.Core.Queries;

public class ReportQueries : IReportQueries
{
    private readonly ReportContext _ctx;

    public ReportQueries(ReportContext context)
    {
        _ctx = context;
    }

    public Task<bool> HasReportedUser(Guid userId, Guid reportedUserId)
    {
        return _ctx.UserReportInstances
            .AsNoTracking()
            .Include(x => x.UserReport)
            .AnyAsync(x => x.UserId == userId && x.UserReport.UserId == reportedUserId);
    }

    public Task<bool> HasReportedPicture(Guid userId, Guid reportedPictureId)
    {
        return _ctx.PictureReportInstances
            .AsNoTracking()
            .Include(x => x.PictureReport)
            .AnyAsync(x => x.UserId == userId && x.PictureReport.PictureId == reportedPictureId);
    }

    public async Task<PagedList<T>> Lookup<T, I>(IPagedListRequest<ReportQueryOptions> pageRequest) where T : Report<I> where I : ReportInstance
    {
        // Default options
        var options = pageRequest.Options ?? new ReportQueryOptions();

        // Build where clause
        Expression<Func<T, bool>> where = _ => true;

        // Actioned
        if (options.Actioned.HasValue)
        {
            where = where.And(x => x.ActionId.HasValue == options.Actioned.Value);
        }

        // Return in given order
        Expression<Func<T, object>> orderBy = null;
        switch (options.OrderBy)
        {
            case ReportQueryOptions.OrderByEnum.Date:
                orderBy = x => x.Date;
                break;
        }

        return await _ctx.Set<T>()
            .AsNoTracking()
            .Include(x => x.Reports)
            .Include(x => x.Action)
            .QueryAsync(where, pageRequest, orderBy);
    }
}