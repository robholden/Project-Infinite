﻿using System.Security.Claims;

using Library.Core;

namespace Library.Service.Api.Auth;

public static class UserClaimsKeys
{
    public static string UserIdentifier => ClaimTypes.NameIdentifier;

    public static string Username => ClaimTypes.Name;

    public static string Email => ClaimTypes.Email;

    public static string AuthToken => "AuthToken";

    public static string Identity => "IdentityKey";

    public static string Platform => nameof(Platform);

    public static string HashToken => nameof(HashToken);

    public static string TwoFactor => "2FA";

    public static string ExtProvider => "Provider";

    public static string ExtProviderIdentifier => "ProviderId";

    public static LoggedInUser GetUser(this ClaimsPrincipal user)
    {
        // We don't expect a user when not authenticated
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var isAdmin = user?.IsInRole(nameof(UserLevel.Admin)) == true;
        var isMod = user?.IsInRole(nameof(UserLevel.Moderator)) == true;

        return new()
        {
            Id = user?.GetGuidClaim(UserIdentifier) ?? new Guid(),
            AuthTokenId = user?.GetGuidClaim(AuthToken) ?? new Guid(),
            Email = user?.GetClaim(Email),
            Username = user?.GetClaim(Username),
            HashToken = user?.GetClaim(HashToken),
            ExternalProviderIdentifier = user?.GetClaim(ExtProviderIdentifier),
            ExternalProvider = user?.GetClaim(ExtProvider)?.ToEnum(ExternalProvider.Unset) ?? ExternalProvider.Unset,
            Level = isAdmin ? UserLevel.Admin : (isMod ? UserLevel.Moderator : UserLevel.Default)
        };
    }

    public static UserRecord ToRecord(this LoggedInUser user) => user == null ? null : new(user.Id, user.Username);
}

public class LoggedInUser
{
    public Guid Id { get; init; }
    public Guid AuthTokenId { get; init; }
    public string HashToken { get; init; }
    public string Username { get; init; }
    public string Email { get; init; }
    public UserLevel Level { get; init; }
    public string ExternalProviderIdentifier { get; init; }
    public ExternalProvider ExternalProvider { get; init; }

    public IUser AsRecord() => new UserRecord(Id, Username);

    public LoggedInUser Secured()
    {
        if (Id == new Guid() || AuthTokenId == new Guid())
        {
            throw new SiteException(ErrorCode.SessionTokenInvalid, System.Net.HttpStatusCode.Forbidden);
        }

        return this;
    }
}