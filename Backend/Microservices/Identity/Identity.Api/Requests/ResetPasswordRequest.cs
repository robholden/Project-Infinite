using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class ResetPasswordRequest
{
    [Required, MinLength(6), MaxLength(255)]
    public string Password { get; set; }

    public bool Clear { get; set; }
}