namespace Identity.Core.Services.Auth;

public interface IExternalLoginProvider
{
    Task<ExternalProviderUser> Login(string token);
}

public record ExternalProviderUser(string Name, string Email, string Identifier);