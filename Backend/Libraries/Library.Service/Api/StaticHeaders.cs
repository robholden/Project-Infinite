namespace Library.Service.Api;

public static class StaticHeaders
{
    public static string IdentityKey => "x-identity-key";

    public static string SiteToken => "x-site-token";

    public static string RecaptchaToken => "x-recaptcha-token";

    public static string Platform => "x-platform";
}