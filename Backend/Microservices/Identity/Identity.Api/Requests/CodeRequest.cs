using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class CodeRequest
{
    [Required, StringLength(10)]
    public string Code { get; set; }

    public bool? DoNotAskAgain { get; set; }
}