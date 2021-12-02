using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Requests;

public class PasswordRequest
{
    [MinLength(6), MaxLength(255)]
    public string Password { get; set; }

    public Guid? TouchIdKey { get; set; }
}

public class PasswordRequest<T> : PasswordRequest where T : class
{
    [Required]
    public T Command { get; set; }
}