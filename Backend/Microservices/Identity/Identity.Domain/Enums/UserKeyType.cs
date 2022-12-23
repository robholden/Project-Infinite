namespace Identity.Domain;

public enum UserKeyType
{
    PasswordReset = 1,
    TwoFactor,
    ConfirmEmail,
    MarketingOptOut
}