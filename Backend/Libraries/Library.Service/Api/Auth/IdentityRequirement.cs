
using Library.Core;
using Library.Service.Api.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Library.Service.Api;

public class IdentityAuthorizeHandler : AuthorizationHandler<IdentityRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityAuthorizeHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IdentityRequirement requirement)
    {
        try
        {
            // Find identity header
            var headers = _httpContextAccessor?.HttpContext?.Request?.Headers ?? throw new Exception("No Headers");
            if (!headers.TryGetValue(StaticHeaders.IdentityKey, out var headerIdentityKey) && headerIdentityKey.Count > 0)
            {
                throw new Exception("Missing identity header");
            }

            // Ensure identity keys are a match
            var identityKey = context.User.GetClaim(UserClaimsKeys.Identity);
            if (headerIdentityKey[0] != identityKey)
            {
                throw new Exception("Identity mismatch");
            }

            // Validate 2FA
            if (!requirement.Ignore2FA && context.User.GetClaim(UserClaimsKeys.TwoFactor) != "passed")
            {
                throw new Exception("Two-factor required");
            }

            // Bind user to application state
            context.Succeed(requirement);
        }
        catch
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

public class IdentityRequirement : IAuthorizationRequirement
{
    public IdentityRequirement(bool ignore2FA)
    {
        Ignore2FA = ignore2FA;
    }

    public bool Ignore2FA { get; set; }
}