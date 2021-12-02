using System.ComponentModel.DataAnnotations;

using Library.Core.Enums;

namespace Report.Api.Requests;

public class ReportActionedRequest
{
    [Required]
    public ReportedAction Action { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; }

    public bool SendEmail { get; set; }
}