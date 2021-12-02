using Library.Core;

using Reports.Domain;

namespace Reports.Core.Queries;

public interface IReportQueries
{
    Task<bool> HasReportedUser(Guid userId, Guid reportedUserId);

    Task<bool> HasReportedPicture(Guid userId, Guid reportedPictureId);

    Task<PagedList<T>> Lookup<T, I>(IPagedListRequest<ReportQueryOptions> pageRequest) where T : Report<I> where I : ReportInstance;
}
