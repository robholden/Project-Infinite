using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class TokenRefreshRequest
{
    [Required]
    public string BearerToken { get; set; }

    [Required]
    public string RefreshToken { get; set; }
}