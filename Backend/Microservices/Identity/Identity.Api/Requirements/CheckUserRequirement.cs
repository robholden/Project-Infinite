
using Identity.Core.Queries;
using Identity.Domain;

using Library.Core;
using Library.Service.Api.Auth;

using Microsoft.AspNetCore.Authorization;

namespace Identity.Api.Requirements;

public class CheckUserAuthorizeHandler : AuthorizationHandler<CheckUserRequirement>
{
    private readonly IAuthTokenQueries _authTokensQuery;

    public CheckUserAuthorizeHandler(IAuthTokenQueries authTokensQuery)
    {
        _authTokensQuery = authTokensQuery;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CheckUserRequirement requirement)
    {
        try
        {
            // Get claim data
            var userId = context.User.GetGuidClaim(UserClaimsKeys.UserIdentifier);
            var identityKey = context.User.GetClaim(UserClaimsKeys.Identity);
            var platform = context.User.GetClaim(UserClaimsKeys.Platform);

            // Fetch record for given token and verify the refresh token
            var token = (await _authTokensQuery.Get(userId.Value, identityKey, platform)) ?? throw new Exception("Missing token");

            // Ensure user is enabled
            if (token.User.Status != UserStatus.Enabled)
            {
                throw new Exception("User is not enabled");
            }

            // Bind user to application state
            context.Succeed(requirement);
        }
        catch
        {
            context.Fail();
        }
    }
}

public class CheckUserRequirement : IAuthorizationRequirement
{
    public CheckUserRequirement()
    {
    }
}