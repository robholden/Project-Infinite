using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class IdRequest
{
    [Required]
    public Guid Id { get; set; }
}