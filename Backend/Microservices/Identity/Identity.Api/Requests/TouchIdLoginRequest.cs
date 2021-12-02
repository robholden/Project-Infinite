using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class TouchIdLoginRequest
{
    [Required]
    public Guid Key { get; set; }
}