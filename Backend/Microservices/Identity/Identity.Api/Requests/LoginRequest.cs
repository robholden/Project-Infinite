using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class LoginRequest
{
    [Required, MinLength(3), MaxLength(255)]
    public string Username { get; set; }

    [Required, MinLength(6), MaxLength(255)]
    public string Password { get; set; }
}