using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class ExternalProviderLoginRequest
{
    [Required]
    public string Token { get; set; }

    public string Name { get; set; }
}