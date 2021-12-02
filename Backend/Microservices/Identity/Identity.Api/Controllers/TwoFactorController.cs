
using AutoMapper;

using Identity.Api.Dtos;
using Identity.Api.Requests;
using Identity.Core;
using Identity.Core.Services;
using Identity.Domain;

using Library.Core;
using Library.Service.Api;
using Library.Service.PubSub;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Identity.Api.Controllers.TwoFactor;

[Route("2FA")]
[Authorize]
public class TwoFactorController : BaseController<TwoFactorController>
{
    private readonly IdentitySettings _settings;

    private readonly IAuthService _authService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IUserService _userService;

    private readonly ISocketsPubSub _sockets;

    public TwoFactorController(
        ILogger<TwoFactorController> logger,
        IMapper mapper,
        IOptions<IdentitySettings> options,
        IAuthService authService,
        IUserService userService,
        ITwoFactorService twoFactorService,
        ISocketsPubSub sockets
    ) : base(logger, mapper)
    {
        _settings = options.Value;

        _authService = authService;
        _userService = userService;
        _twoFactorService = twoFactorService;

        _sockets = sockets;
    }

    [ReCaptcha]
    [HttpPost("disable")]
    public async Task Disable([FromBody] PasswordRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Verify password
        await _authService.VerifyAuth(LoggedInUser.Id, new(request.Password, request.TouchIdKey));

        // Disable 2FA
        var user = await _userService.Disable2FA(LoggedInUser.Id);

        // Send socket update to refresh user
        _ = _sockets.UpdatedUserField(new(LoggedInUser.Id, nameof(UserDto.TwoFactorEnabled), user.TwoFactorEnabled));
    }

    [ReCaptcha]
    [HttpPost("enable")]
    public async Task<IEnumerable<RecoveryCodeDto>> Enable([FromBody] CodeRequest request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Enable 2FA
        var user = await _userService.Enable2FA(LoggedInUser.Id, request.Code);

        // Generate recovery codes
        var codes = await _twoFactorService.GenerateRecoveryCodes(LoggedInUser.Id);

        // Send socket update to refresh user
        _ = _sockets.UpdatedUserFields(new(LoggedInUser.Id, new() { { nameof(UserDto.TwoFactorEnabled), user.TwoFactorEnabled }, { nameof(UserDto.TwoFactorType), user.TwoFactorType } }));

        return _mapper.Map<IEnumerable<RecoveryCodeDto>>(codes);
    }

    [ReCaptcha]
    [HttpPost("setup")]
    public async Task<object> Setup([FromBody] PasswordRequest<Setup2FARequest> request)
    {
        // Verify model state
        ThrowWhenStateIsInvalid();

        // Verify params
        var type = request.Command.Type.ToEnum(TwoFactorType.Unset);
        if (type == TwoFactorType.Unset)
        {
            ThrowNotFound();
        }

        // Verify password
        await _authService.VerifyAuth(LoggedInUser.Id, new(request.Password, request.TouchIdKey));

        // Setup 2FA
        var secret = await _userService.Setup2FA(LoggedInUser.Id, type, request.Command.Mobile);

        return new
        {
            Secret = secret,
            Mode = _settings.Totp.Mode.ToString().ToUpper(),
            _settings.Totp.Size,
            _settings.Totp.Step
        };
    }
}