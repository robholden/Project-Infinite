using Library.Core;

using Reports.Domain;

namespace Reports.Core.Queries;

public interface IReportQueries
{
    Task<bool> HasReportedUser(Guid userId, Guid reportedUserId);

    Task<PagedList<T>> Lookup<T, I>(IPagedListRequest<ReportQueryOptions> pageRequest) where T : Report<I> where I : ReportInstance;
}
