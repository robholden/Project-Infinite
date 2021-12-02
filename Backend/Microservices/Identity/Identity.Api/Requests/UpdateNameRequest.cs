using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class UpdateNameRequest
{
    [Required, MaxLength(255)]
    public string Name { get; set; }
}