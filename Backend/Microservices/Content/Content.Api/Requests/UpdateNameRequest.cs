using System.ComponentModel.DataAnnotations;

namespace Content.Api.Requests;

public class UpdateNameRequest
{
    [Required]
    public string Name { get; set; }
}