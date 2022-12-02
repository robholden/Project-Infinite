using Library.Core;

namespace Identity.Domain.Dtos;

public class SimpleUserDto
{
    public string Username { get; set; }
}

public class UserDto : SimpleUserDto
{
    public string Name { get; set; }

    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public string Mobile { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public int TwoFactorType { get; set; }

    public bool AcceptedTerms { get; set; }

    public DateTime? LastActive { get; set; }

    public DateTime Created { get; set; }
}

public class AdminUserDto : UserDto
{
    public Guid UserId { get; set; }

    public UserLevel UserLevel { get; set; }
}