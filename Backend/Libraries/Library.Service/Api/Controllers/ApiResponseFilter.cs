using Library.Core;
using Library.Service.Api.Auth;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Library.Service.Api;

public sealed class ApiResponseFilter : ExceptionFilterAttribute
{
    private readonly ILogger<ApiResponseFilter> _logger;

    public ApiResponseFilter(ILogger<ApiResponseFilter> logger)
    {
        _logger = logger;
    }

    public override void OnException(ExceptionContext context)
    {
        var result = context.Exception.AsObjectResult();
        var statusCode = result.StatusCode ?? StatusCodes.Status400BadRequest;
        var user = context.HttpContext.User?.GetUser();

        if (context.Exception is SiteException siteEx)
        {
            statusCode = siteEx.StatusCode ?? statusCode;
            _logger.LogWarning("{Username} got a handled exception => {StatusCode}: {Error}", user?.Username ?? "??", statusCode, siteEx.ErrorCode.ToString() ?? "??");
        }
        else
        {
            _logger.LogError(context.Exception, "{Username} threw internal exception => {Message}", user?.Username ?? "??", context.Exception.Message);
        }

        context.Result = result;
        context.HttpContext.Response.StatusCode = statusCode;
    }
}