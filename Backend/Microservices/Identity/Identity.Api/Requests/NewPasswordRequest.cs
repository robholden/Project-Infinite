using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class NewPasswordRequest
{
    [Required, MinLength(6), MaxLength(255)]
    public string NewPassword { get; set; }

    [Required, MinLength(6), MaxLength(255)]
    public string OldPassword { get; set; }
}