using Library.Core;

namespace Reports.Core.Queries;

public class ReportQueryOptions : IPageListQuery<ReportQueryOptions.OrderByEnum>
{
    public enum OrderByEnum
    {
        None,
        Date
    }

    public bool? Actioned { get; set; }

    public OrderByEnum OrderBy { get; set; }
}