using System.ComponentModel.DataAnnotations;

using Library.Core;

namespace Identity.Api.Requests;

public class ReportActionedRequest
{
    [Required]
    public Guid ReportId { get; set; }

    [Required]
    public ReportedAction Action { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; }
}