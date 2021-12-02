using System.Net;

using AutoMapper;

using Library.Core;
using Library.Core.Enums;
using Library.Core.Models;
using Library.Service.Api.Auth;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Library.Service.Api;

[PreventDDOS]
[Route("[controller]")]
[ApiController]
[ServiceFilter(typeof(ApiResponseFilter))]
public class BaseController<T> : ControllerBase
{
    protected internal readonly ILogger _logger;
    protected internal readonly IMapper _mapper;

    private LoggedInUser _user;

    public BaseController()
    {
    }

    public BaseController(ILogger<T> logger, IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
    }

    protected bool LoggedIn => LoggedInUser != null;

    protected LoggedInUser LoggedInUser => _user ??= User.GetUser();

    protected bool IsAdmin => User?.IsInRole(nameof(UserLevel.Admin)) == true;

    protected bool IsMod => User?.IsInRole(nameof(UserLevel.Moderator)) == true;

    protected string HeaderIdentityKey
    {
        get
        {
            if (HttpContext.TryGetHeader(StaticHeaders.IdentityKey, out var key)) return key;
            return null;
        }
    }

    protected string IpAddress => HttpContext.Connection.RemoteIpAddress?.ToString();

    protected string HeaderPlatform
    {
        get
        {
            if (HttpContext.TryGetHeader(StaticHeaders.Platform, out var key)) return key;
            return null;
        }
    }

    protected ClientIdentity ClientIdentity => new(HeaderIdentityKey, IpAddress, HeaderPlatform);

    protected void ThrowPermissionException()
    {
        Throw(HttpStatusCode.Unauthorized, new SiteException(ErrorCode.MissingPermissions));
    }

    protected void ThrowWhenStateIsInvalid()
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Keys
                .SelectMany(key => ModelState[key].Errors.Select(x => x.ErrorMessage))
                .ToList();

            Throw(HttpStatusCode.BadRequest, new SiteException(ErrorCode.InvalidModelRequest, errors.FirstOrDefault()));
        }
    }

    protected void Throw(ErrorCode errorCode) => Throw(HttpStatusCode.BadRequest, new SiteException(errorCode));

    protected void ThrowNotFound() => Throw(HttpStatusCode.NotFound);

    protected void ThrowUnauthorized(SiteException siteException = null) => Throw(HttpStatusCode.Unauthorized, siteException);

    protected ApiSiteException Throw(HttpStatusCode statusCode, SiteException siteException = null)
    {
        throw new ApiSiteException((int)statusCode, siteException);
    }
}