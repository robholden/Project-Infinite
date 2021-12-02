using OtpNet;

namespace Identity.Core;

public class IdentitySettings
{
    public TotpSettings Totp { get; set; }

    public AuthenticationSettings OAuthProviders { get; set; }

    public int ExpiryLength { get; set; }

    public int FailedLoginAttempts { get; set; }

    public int FailedLoginDuration { get; set; }
}

public class TotpSettings
{
    public int Step { get; set; }

    public int Size { get; set; }

    public OtpHashMode Mode { get; set; }

    public int ExpiresInSeconds { get; set; }
}

public class AuthenticationSettings
{
    public string[] AppleClientIds { get; set; }

    public string[] GoogleClientIds { get; set; }

    public string FacebookClientSecret { get; set; }
}