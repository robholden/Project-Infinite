
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Library.Service.Api.Auth;

internal class IdentityPolicyProvider : IAuthorizationPolicyProvider
{
    public IdentityPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        // ASP.NET Core only uses one authorization policy provider, so if the custom
        // implementation doesn't handle all policies (including default policies, etc.) it
        // should fall back to an alternate provider.
        //
        // In this sample, a default authorization policy provider (constructed with options from
        // the dependency injection container) is used if this custom provider isn't able to
        // handle a given policy name.
        //
        // If a custom policy provider is able to handle all expected policy names then, of
        // course, this fallback pattern is unnecessary.
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

    // Policies are looked up by string name, so expect 'parameters' (like age) to be embedded in
    // the policy names. This is abstracted away from developers by the more strongly-typed
    // attributes derived from AuthorizeAttribute (like [MinimumAgeAuthorize] in this sample)
    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        var policy = new AuthorizationPolicyBuilder();
        policy.AddRequirements(new IdentityRequirement(policyName == "Ignore2FA"));

        return Task.FromResult(policy.Build());
    }
}