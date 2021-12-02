using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class RegisterExternalRequest
{
    public bool AllowMarketing { get; set; }

    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [DataType(DataType.PhoneNumber)]
    public string Mobile { get; set; }
}