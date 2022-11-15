
using Library.Core;

using Reports.Domain;

namespace Reports.Core.Services;

public interface IReportService
{
    Task<ReportAction> ActionUserReport(Guid reportId, ReportedAction actionTaken, IUser actionedBy, string notes, bool sendEmail);

    Task<ReportAction> ActionPictureReport(Guid reportId, ReportedAction actionTaken, IUser actionedBy, string notes, bool sendEmail);
}