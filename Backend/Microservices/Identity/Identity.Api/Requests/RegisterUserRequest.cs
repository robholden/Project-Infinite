using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class RegisterUserRequest
{
    public bool AllowMarketing { get; set; }

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; }

    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; set; }

    [DataType(DataType.PhoneNumber)]
    public string Mobile { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required, MinLength(6), MaxLength(255)]
    public string Password { get; set; }
}