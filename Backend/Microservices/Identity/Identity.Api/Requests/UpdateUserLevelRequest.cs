using System.ComponentModel.DataAnnotations;

using Library.Core;

namespace Identity.Api.Requests;

public class UpdateUserLevelRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public UserLevel Level { get; set; }
}