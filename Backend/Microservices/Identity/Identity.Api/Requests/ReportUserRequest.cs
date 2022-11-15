using System.ComponentModel.DataAnnotations;

using Library.Core;

namespace Identity.Api.Requests;

public class ReportUserRequest
{
    [Required]
    public ReportUserReason Reason { get; set; }
}