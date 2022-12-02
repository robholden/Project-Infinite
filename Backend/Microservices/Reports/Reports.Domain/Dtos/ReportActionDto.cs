
using Library.Core;

namespace Reports.Domain.Dtos;

public class ReportActionDto
{
    public string Username { get; set; }

    public ReportedAction Action { get; set; }

    public string Notes { get; set; }

    public DateTime Created { get; set; }
}