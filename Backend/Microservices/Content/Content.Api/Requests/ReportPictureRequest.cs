using System.ComponentModel.DataAnnotations;

using Library.Core.Enums;

namespace Content.Api.Requests;

public class ReportPictureRequest
{
    [Required]
    public ReportPictureReason Reason { get; set; }
}