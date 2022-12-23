
using AutoMapper;

using Identity.Api.Requests;
using Identity.Core;
using Identity.Core.Services;
using Identity.Domain;

using Library.Service.Api;
using Library.Service.PubSub;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

public class PasswordController : BaseController<PasswordController>
{
    private readonly IAuthService _authService;
    private readonly IPasswordService _passwordService;
    private readonly IUserKeyService _userKeyService;
    private readonly IUserService _userService;

    private readonly ISocketsPubSub _sockets;

    public PasswordController(
        ILogger<PasswordController> logger,
        IMapper mapper,
        IAuthService authService,
        IPasswordService passwordService,
        IUserService userService,
        IUserKeyService userKeyService,
        ISocketsPubSub sockets
    ) : base(logger, mapper)
    {
        _authService = authService;
        _passwordService = passwordService;
        _userService = userService;
        _userKeyService = userKeyService;

        _sockets = sockets;
    }

    [ReCaptcha]
    [Authorize]
    [HttpPut("change")]
    public async Task Change([FromBody] NewPasswordRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Send request to service
        await _passwordService.Change(LoggedInUser.Id, request.OldPassword, request.NewPassword);
    }

    [HttpPost("forgot")]
    public async Task Forgot([FromBody] ForgotPasswordRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Send request to service
        try
        {
            await _userService.ForgotPassword(request.Email);
        }
        catch (Exception ex)
        {
            // Always assume success
            _logger.LogError(ex, "Invalid forgot password email: {Email}", request.Email);
        }
    }

    [ReCaptcha]
    [HttpPut("reset/{key}")]
    public async Task Reset([FromRoute] string key, [FromBody] ResetPasswordRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Send request to service
        var user = await _userService.VerifyAndResetPassword(key, request.Password);

        // Clear all login sessions?
        if (request.Clear)
        {
            await _authService.DeleteAll(user.UserId);
            await _sockets.RevokeSession(user.UserId);
        }
    }

    [HttpGet("reset/{key}")]
    public async Task ValidateResetKey([FromRoute] string key)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Send request to service
        await _userKeyService.ValidateKey(key, UserKeyType.PasswordReset);
    }
}