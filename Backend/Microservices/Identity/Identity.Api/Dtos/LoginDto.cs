
using Identity.Domain;

using Library.Core.Enums;

namespace Identity.Api.Dtos;

public class LoginDto
{
    public Guid? AuthToken { get; set; }

    public string Token { get; set; }

    public string RefreshToken { get; set; }

    public UserDto User { get; set; }

    public TwoFactorType TwoFactorRequired { get; set; }

    public ExternalProvider Provider { get; set; }

    public int ExpiresIn { get; set; }
}