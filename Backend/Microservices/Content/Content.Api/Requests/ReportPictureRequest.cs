using System.ComponentModel.DataAnnotations;

using Library.Core;

namespace Content.Api.Requests;

public class ReportPictureRequest
{
    [Required]
    public ReportPictureReason Reason { get; set; }
}