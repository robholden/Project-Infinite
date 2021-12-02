
using Library.Core.Enums;
using Library.Core.Models;

namespace Reports.Domain;

public class UserReport : Report<UserReportInstance>, IUser
{
    public UserReport()
    {
    }

    public UserReport(IUser user, string name, string email) : base(user)
    {
        Name = name;
        Email = email;
    }

    public string Name { get; set; }

    public string Email { get; set; }
}

public class UserReportInstance : ReportInstance
{
    public UserReportInstance()
    {
    }

    public UserReportInstance(IUser user, ReportUserReason reason) : base(user)
    {
        Reason = reason;
    }

    public ReportUserReason Reason { get; set; }

    public Guid ReportId { get; set; }

    public virtual UserReport UserReport { get; set; }
}