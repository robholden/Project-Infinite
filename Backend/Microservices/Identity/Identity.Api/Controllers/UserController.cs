using System.Net;

using AutoMapper;

using Identity.Api.Requests;
using Identity.Core;
using Identity.Core.Queries;
using Identity.Core.Services;
using Identity.Domain.Dtos;

using Library.Core;
using Library.Service.Api;
using Library.Service.Api.Auth;
using Library.Service.PubSub;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

public class UserController : BaseController<UserController>
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;
    private readonly IUserPreferenceService _prefService;

    private readonly IUserQueries _userQueries;

    private readonly IReportPubSub _reportEvents;
    private readonly ISocketsPubSub _socketEvents;

    public UserController(
        ILogger<UserController> logger,
        IMapper mapper,
        IAuthService authService,
        IUserService userService,
        IUserPreferenceService prefService,
        IUserQueries userQueries,
        IReportPubSub reportEvents,
        ISocketsPubSub socketsEvents
    )
        : base(logger, mapper)
    {
        _authService = authService;
        _userService = userService;
        _prefService = prefService;

        _userQueries = userQueries;

        _socketEvents = socketsEvents;
        _reportEvents = reportEvents;
    }

    [ReCaptcha]
    [HttpPut("confirm-email/{key}")]
    public async Task ConfirmEmail([FromRoute] string key)
    {
        // Validate and mark email as confirmed
        var user = await _userService.VerifyAndConfirmEmail(key);

        // Send socket update to UI
        _ = _socketEvents.UpdatedUserFields(user.UserId, new UserFieldChange(nameof(UserDto.EmailConfirmed), user.EmailConfirmed));
    }

    [HttpGet("{username}")]
    public async Task<SimpleUserDto> GetByUsername([FromRoute] string username)
    {
        var user = await _userQueries.GetByUsername(username);
        if (user == null)
        {
            Throw(HttpStatusCode.NotFound);
        }

        if (IsMod)
        {
            return _mapper.Map<AdminUserDto>(user);
        }

        return _mapper.Map<SimpleUserDto>(user);
    }

    [Authorize(Roles = nameof(UserLevel.Moderator))]
    [HttpGet("id/{userId}")]
    public async Task<SimpleUserDto> GetByGuid([FromRoute] Guid userId)
    {
        var user = await _userQueries.Get(userId);
        if (user == null)
        {
            Throw(HttpStatusCode.NotFound);
        }

        return _mapper.Map<UserDto>(user);
    }

    [Authorize]
    [HttpGet]
    public async Task<UserDto> Get()
    {
        var user = await _userQueries.Get(LoggedInUser.Id);
        if (user == null)
        {
            Throw(HttpStatusCode.NotFound);
        }

        return _mapper.Map<UserDto>(user);
    }

    [ReCaptcha]
    [HttpPost]
    public async Task<UserDto> Register([FromBody] RegisterUserRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Send request to service
        var user = await _userService.Register(new RegisterRequest(request.Username, request.Name, request.Email, request.Password, request.AllowMarketing, request.Mobile));

        return _mapper.Map<UserDto>(user);
    }

    [Authorize]
    [ReCaptcha]
    [HttpPost("register/{provider}")]
    public async Task<UserDto> RegisterExternalProvider([FromRoute] string provider, [FromBody] RegisterExternalRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Verify providers match
        var claimProvider = LoggedInUser.ExternalProvider;
        if (claimProvider == ExternalProvider.Unset || claimProvider != provider.ToEnum(ExternalProvider.Unset))
        {
            ThrowNotFound();
        }

        // Send request to service
        var providerInfo = new ExternalProviderRequest(claimProvider, LoggedInUser.ExternalProviderIdentifier);
        var registration = new RegisterRequest(request.Username, request.Name, LoggedInUser.Email, "", request.AllowMarketing, request.Mobile, providerInfo);
        var user = await _userService.Register(registration);

        return _mapper.Map<UserDto>(user);
    }

    [Authorize]
    [HttpPost("{username}/report")]
    public async Task Report([FromRoute] string username, [FromBody] ReportUserRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Ensure this user exists
        var user = await _userQueries.GetByUsername(username);
        if (user == null)
        {
            Throw(HttpStatusCode.NotFound);
        }

        // Make sure not to report themselves
        if (user.UserId == LoggedInUser.Id)
        {
            return;
        }

        // Publish report event
        await _reportEvents.ReportUser(new(LoggedInUser.ToRecord(), request.Reason, user.ToUserRecord(), user.Name, user.Email));
    }

    [Authorize]
    [HttpPut("resend-confirm-email")]
    public async Task ResendConfirmEmail()
    {
        await _userService.SendEmailConfirmation(LoggedInUser.Id);
    }

    [ReCaptcha]
    [Authorize]
    [HttpPut("update")]
    public async Task Update([FromBody] PasswordRequest<UpdateUserRequest> request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Track which fields to update
        var changes = new List<UserFieldChange>();

        // Verify password for email & username
        if (!string.IsNullOrEmpty(request.Command.Email) || !string.IsNullOrEmpty(request.Command.Username))
        {
            if (LoggedInUser.ExternalProvider == ExternalProvider.Unset)
            {
                await _authService.VerifyAuth(LoggedInUser.Id, new(request.Password, request.TouchIdKey));
            }

            if (string.IsNullOrEmpty(request.Command.Username))
            {
                await _userService.UpdateUsername(LoggedInUser.Id, request.Command.Username);
                changes.Add(new(nameof(UserDto.Username), request.Command.Username));
            }

            if (string.IsNullOrEmpty(request.Command.Email))
            {
                await _userService.UpdateEmail(LoggedInUser.Id, request.Command.Email);
                changes.Add(new(nameof(UserDto.Email), request.Command.Email));
            }
        }

        if (!string.IsNullOrEmpty(request.Command.Name))
        {
            await _userService.UpdateName(LoggedInUser.Id, request.Command.Name);
            changes.Add(new(nameof(UserDto.Name), request.Command.Name));
        }

        if (!string.IsNullOrEmpty(request.Command.Mobile))
        {
            await _userService.UpdateMobile(LoggedInUser.Id, request.Command.Mobile);
            changes.Add(new(nameof(UserDto.Mobile), request.Command.Mobile));
        }

        // Send socket update to update user properties
        if (changes.Count > 0)
        {
            _ = _socketEvents.UpdatedUserFields(LoggedInUser.Id, changes.ToArray());
        }
    }

    [ReCaptcha]
    [Authorize]
    [HttpPut("update/username")]
    public async Task UpdateUsername([FromBody] PasswordRequest<UpdateUsernameRequest> request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Verify password
        if (LoggedInUser.ExternalProvider == ExternalProvider.Unset)
        {
            await _authService.VerifyAuth(LoggedInUser.Id, new(request.Password, request.TouchIdKey));
        }

        // Send request to service
        await _userService.UpdateUsername(LoggedInUser.Id, request.Command.Username);

        // Send socket update to refresh user
        _ = _socketEvents.UpdatedUserFields(LoggedInUser.Id, new UserFieldChange(nameof(UserDto.Username), request.Command.Username));
    }

    [ReCaptcha]
    [Authorize]
    [HttpPut("update/email")]
    public async Task UpdateEmail([FromBody] PasswordRequest<UpdateEmailRequest> request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Verify password
        if (LoggedInUser.ExternalProvider == ExternalProvider.Unset)
        {
            await _authService.VerifyAuth(LoggedInUser.Id, new(request.Password, request.TouchIdKey));
        }

        // Send request to service
        await _userService.UpdateEmail(LoggedInUser.Id, request.Command.Email);

        // Send socket update to refresh user
        _ = _socketEvents.UpdatedUserFields(LoggedInUser.Id, new UserFieldChange(nameof(UserDto.Email), request.Command.Email));
    }

    [ReCaptcha]
    [Authorize]
    [HttpPut("update/mobile")]
    public async Task UpdateMobile([FromBody] UpdateMobileRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Send request to service
        await _userService.UpdateMobile(LoggedInUser.Id, request.Mobile);

        // Send socket update to refresh user
        _ = _socketEvents.UpdatedUserFields(LoggedInUser.Id, new UserFieldChange(nameof(UserDto.Mobile), request.Mobile));
    }

    [ReCaptcha]
    [Authorize]
    [HttpPut("update/name")]
    public async Task UpdateName([FromBody] UpdateNameRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Send request to service
        await _userService.UpdateName(LoggedInUser.Id, request.Name);

        // Send socket update to refresh user
        _ = _socketEvents.UpdatedUserFields(LoggedInUser.Id, new UserFieldChange(nameof(UserDto.Name), request.Name));
    }

    [HttpPut("unsubscribe/{key}")]
    public Task Unsubscribe([FromRoute] Guid key)
    {
        return _prefService.UnsubscribeFromMarketing(key);
    }

    [ReCaptcha]
    [Authorize(Policy = "CheckUser")]
    [HttpPost("account/delete")]
    public async Task DeleteAccount([FromBody] PasswordRequest request)
    {
        // Verify password when using internal login
        if (LoggedInUser.ExternalProvider == ExternalProvider.Unset)
        {
            // Verify model state
            ThrowWhenStateIsInvalid();

            await _authService.VerifyAuth(LoggedInUser.Id, new(request.Password, request.TouchIdKey));
        }

        // Send request to service
        await _userService.DeleteAccount(LoggedInUser.Id);
    }

    [ReCaptcha]
    [Authorize(Policy = "CheckUser", Roles = nameof(UserLevel.Admin))]
    [HttpPut("update/level")]
    public async Task UpdateLevel([FromBody] UpdateUserLevelRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // DO NOT ALLOW ADMIN
        if (request.Level == UserLevel.Admin)
        {
            Throw(HttpStatusCode.Forbidden);
        }

        // Send request to service
        await _userService.UpdateLevel(request.UserId, request.Level);
    }
}