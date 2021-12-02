using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class UpdateUsernameRequest
{
    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; set; }
}