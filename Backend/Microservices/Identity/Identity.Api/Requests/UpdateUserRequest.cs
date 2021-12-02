using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class UpdateUserRequest
{
    [MaxLength(100)]
    public string Name { get; set; }

    [EmailAddress, MaxLength(255)]
    public string Email { get; set; }

    [MaxLength(50)]
    public string Username { get; set; }

    [DataType(DataType.PhoneNumber)]
    public string Mobile { get; set; }
}