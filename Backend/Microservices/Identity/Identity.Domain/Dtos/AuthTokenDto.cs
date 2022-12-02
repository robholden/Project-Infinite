namespace Identity.Domain.Dtos;

public class AuthTokenDto
{
    public Guid AuthTokenId { get; set; }

    public bool Active { get; set; }

    public DateTime Created { get; set; }

    public bool Expired { get; set; }

    public DateTime Expires { get; set; }

    public string IpAddress { get; set; }

    public string Platform { get; set; }

    public bool TwoFactorPassed { get; set; }

    public DateTime? Updated { get; set; }

    public string HashToken { get; set; }
}