using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using AutoMapper;

using Identity.Api.Dtos;
using Identity.Api.Requests;
using Identity.Core;
using Identity.Core.Queries;
using Identity.Core.Services;
using Identity.Core.Services.Auth;
using Identity.Core.Services.Auth.Providers;
using Identity.Domain;

using Library.Core;
using Library.Core.Enums;
using Library.Service;
using Library.Service.Api;
using Library.Service.Api.Auth;
using Library.Service.PubSub;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Api.Controllers.Auth;

public class AuthController : BaseController<AuthController>
{
    private readonly SharedSettings _sharedSettings;
    private readonly IdentitySettings _identitySettings;

    private readonly IAuthTokenQueries _tokenQueries;

    private readonly IAuthService _authService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IUserService _userService;

    private readonly ISocketsPubSub _sockets;

    public AuthController(
        ILogger<AuthController> logger,
        IMapper mapper,
        IOptions<SharedSettings> sharedOptions,
        IOptions<IdentitySettings> identityOptions,
        IAuthTokenQueries tokenQueries,
        IAuthService authService,
        IUserService userService,
        ITwoFactorService twoFactorService,
        ISocketsPubSub sockets
    )
        : base(logger, mapper)
    {
        _sharedSettings = sharedOptions.Value;
        _identitySettings = identityOptions.Value;

        _tokenQueries = tokenQueries;

        _authService = authService;
        _twoFactorService = twoFactorService;
        _userService = userService;

        _sockets = sockets;
    }

    [ReCaptcha]
    [LimitRequest(10, 15)]
    [HttpPost("login")]
    public async Task<LoginDto> Login([FromBody] LoginRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Bind identity
        if (string.IsNullOrEmpty(HeaderIdentityKey))
        {
            ThrowUnauthorized();
        }

        // Send request to service
        var token = await _authService.Login(request.Username, request.Password, ClientIdentity);

        return await CreateToken(token);
    }

    [ReCaptcha]
    [LimitRequest(10, 15)]
    [HttpPost("login/touch-id")]
    public async Task<LoginDto> LoginWithTouchId([FromBody] TouchIdLoginRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Bind identity
        if (string.IsNullOrEmpty(HeaderIdentityKey))
        {
            ThrowUnauthorized();
        }

        // Send request to service
        var token = await _authService.TouchIdLogin(request.Key, ClientIdentity);

        return await CreateToken(token);
    }

    [ReCaptcha]
    [HttpPost("login/{provider}")]
    public async Task<LoginDto> LogInWithExternalProvider([FromRoute] string provider, [FromBody] ExternalProviderLoginRequest request, [FromServices] IMemoryCache cache)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Extract external provider
        var extProvider = provider.ToEnum(ExternalProvider.Unset);
        IExternalLoginProvider loginProvider = extProvider switch
        {
            ExternalProvider.Google => new LoginGoogleProvider(cache, _identitySettings.OAuthProviders.GoogleClientIds),
            ExternalProvider.Facebook => new LoginFacebookProvider(_identitySettings.OAuthProviders.FacebookClientSecret),
            ExternalProvider.Apple => new LoginAppleProvider(cache, _identitySettings.OAuthProviders.AppleClientIds),
            _ => null
        };

        // Verify login from external provider
        var externalUser = await loginProvider?.Login(request.Token);
        if (externalUser == null)
        {
            ThrowUnauthorized();
        }

        // If name is missing, use the request one
        if (string.IsNullOrEmpty(externalUser.Name) && !string.IsNullOrEmpty(request.Name))
        {
            externalUser = new ExternalProviderUser(request.Name, externalUser.Email, externalUser.Identifier);
        }

        // Check for an account
        var token = await _authService.ExternalProviderLogin(externalUser.Identifier, externalUser.Email, extProvider, ClientIdentity);
        if (token != null)
        {
            return await CreateToken(token);
        }

        // Create temporary jwt for registration
        var claims = new List<Claim>()
            {
                new Claim(UserClaimsKeys.ExternalProvider, extProvider.ToString()),
                new Claim(UserClaimsKeys.Email, externalUser.Email),
                new Claim(UserClaimsKeys.ExternalProviderIdentifier, externalUser.Identifier)
            };

        return new LoginDto
        {
            Token = CreateJwt(claims, 120),
            User = new()
            {
                Name = externalUser.Name,
                Email = externalUser.Email
            }
        };
    }

    [Authorize(Policy = "CheckUser")]
    [HttpPost("touch-id/{state}")]
    public async Task<SecretDto> EnableDisableTouchId([FromRoute] string state)
    {
        var token = await _authService.EnableDisableTouchId(LoggedInUser.AuthTokenId, ClientIdentity, state.AreEqual("enabled"));
        return new() { Secret = $"{ token.AuthTokenId }" };
    }

    [HttpGet("logout")]
    public async Task Logout()
    {
        if (!LoggedIn)
        {
            return;
        }

        await _authService.Logout(LoggedInUser.AuthTokenId);
        await _sockets.NewSession(new(LoggedInUser.Id));
    }

    [ResponseCache(Duration = 120, VaryByHeader = "Site-Token")]
    [HttpPost("validate")]
    public async Task<LoginDto> Refresh([FromBody] TokenRefreshRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Bind identity
        if (string.IsNullOrEmpty(HeaderIdentityKey) || string.IsNullOrEmpty(HeaderPlatform))
        {
            ThrowUnauthorized();
        }

        // Validate given bearer token
        var principal = GetPrincipalFromExpiredToken(request.BearerToken);
        var userId = principal.GetGuidClaim(UserClaimsKeys.UserIdentifier);
        var identityKey = principal.GetClaim(UserClaimsKeys.Identity);
        var platform = principal.GetClaim(UserClaimsKeys.Platform);
        var accessKey = principal.GetGuidClaim(UserClaimsKeys.AuthToken);

        // Ensure this token is theirs
        if (userId == null || HeaderIdentityKey != identityKey || HeaderPlatform != platform)
        {
            ThrowUnauthorized();
        }

        // Refresh token and return
        var token = await _authService.RefreshAuthToken(accessKey.Value, request.RefreshToken, true);
        return await CreateToken(token, true);
    }

    [ReCaptcha]
    [Authorize(Policy = "Ignore2FA")]
    [HttpPost("recover-account")]
    public async Task<LoginDto> RecoverAccount([FromBody] CodeRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Attempt to recover account
        await _twoFactorService.Recover(LoggedInUser.Id, request.Code);

        // Generate new token
        var token = await _authService.RefreshAuthToken(LoggedInUser.AuthTokenId, null, false);

        // Create and return response
        return await CreateToken(token);
    }

    [Authorize]
    [Authorize(Policy = "CheckUser")]
    [HttpPost("session/{sessionId}/delete")]
    public async Task Remove([FromRoute] Guid sessionId, [FromBody] PasswordRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Verify password
        if (LoggedInUser.ExternalProvider == ExternalProvider.Unset)
        {
            await _authService.VerifyAuth(LoggedInUser.Id, new(request.Password, request.TouchIdKey));
        }

        // Send request to service
        var token = await _authService.Delete(sessionId);
        if (token != null)
        {
            await _sockets.RevokeSession(new(token.UserId, token.AuthTokenId));
        }
    }

    [Authorize]
    [Authorize(Policy = "CheckUser")]
    [HttpPost("session/delete")]
    public async Task RemoveAll([FromBody] PasswordRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Verify password
        if (LoggedInUser.ExternalProvider == ExternalProvider.Unset)
        {
            await _authService.VerifyAuth(LoggedInUser.Id, new(request.Password, request.TouchIdKey));
        }

        // Send request to service
        await _authService.DeleteAll(LoggedInUser.Id);
        await _sockets.RevokeSession(new(LoggedInUser.Id));
    }

    [Authorize(Policy = "Ignore2FA")]
    [HttpPost("resend-code")]
    public async Task ResendCode()
    {
        await _userService.Trigger2FA(LoggedInUser.Id);
    }

    [Authorize]
    [HttpGet("sessions")]
    public async Task<PagedList<AuthTokenDto>> Sessions([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string orderDir = null)
    {
        // Search sessions
        var options = new AuthTokenQueryOptions { OrderBy = AuthTokenQueryOptions.OrderByEnum.UpdatedDate };
        var tokens = await _tokenQueries.Lookup(new PagedListRequest<AuthTokenQueryOptions>(page, pageSize, orderDir.ToEnum(OrderByDirection.Desc), options));

        return tokens.ToDto<AuthTokenDto>(_mapper);
    }

    [ReCaptcha]
    [Authorize(Policy = "Ignore2FA")]
    [HttpPost("verify")]
    public async Task<LoginDto> Verify([FromBody] CodeRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Verify code
        var token = await _authService.ValidateTwoFactorCode(LoggedInUser.Id, LoggedInUser.AuthTokenId, request.Code, request.DoNotAskAgain ?? false);

        return await CreateToken(token);
    }

    private async Task<LoginDto> CreateToken(AuthToken token, bool isRefresh = false)
    {
        var claims = new List<Claim>()
            {
                new Claim(UserClaimsKeys.Email, token.User.Email),
                new Claim(UserClaimsKeys.UserIdentifier, token.UserId.ToString()),
                new Claim(UserClaimsKeys.Username, token.User.Username),
                new Claim(UserClaimsKeys.AuthToken, token.AuthTokenId.ToString()),
                new Claim(UserClaimsKeys.Identity, token.IdentityKey),
                new Claim(UserClaimsKeys.Platform, token.PlatformRaw),
                new Claim(UserClaimsKeys.TwoFactor, token.TwoFactorPassed ? "passed" : "")
            };

        // Add roles
        for (var i = UserLevel.Default; i < token.User.UserLevel; i++)
        {
            claims.Add(new Claim(ClaimTypes.Role, (i + 1).ToString()));
        }

        // Add external provider
        if (token.User.ExternalProvider != ExternalProvider.Unset)
        {
            claims.Add(new Claim(UserClaimsKeys.ExternalProvider, token.User.ExternalProvider.ToString()));
            claims.Add(new Claim(UserClaimsKeys.ExternalProviderIdentifier, token.User.ExternalProviderIdentifier));
        }

        var dto = new LoginDto
        {
            Token = CreateJwt(claims, _identitySettings.ExpiryLength),
            RefreshToken = token.RefreshToken,
            AuthToken = token.AuthTokenId,
            User = !token.TwoFactorPassed ? null : _mapper.Map<UserDto>(token.User),
            TwoFactorRequired = token.TwoFactorPassed ? TwoFactorType.Unset : token.User.TwoFactorType,
            Provider = token.User.ExternalProvider,
            ExpiresIn = _identitySettings.ExpiryLength
        };

        if (dto.User != null && !isRefresh)
        {
            await _sockets.NewSession(new(token.UserId));
        }

        return dto;
    }

    private string CreateJwt(IList<Claim> claims, int expiryLength)
    {
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_sharedSettings.JwtIssuerKey));
        var jwt = new JwtSecurityToken(
            issuer: "Snow Capture",
            audience: "Everyone",
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddSeconds(expiryLength),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_sharedSettings.JwtIssuerKey));
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        if (securityToken is JwtSecurityToken jwtToken && !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid given token is invalid");
        }

        return principal;
    }
}