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

        _logger.LogError(context.Exception, context.Exception.Message);

        context.Result = result;
        context.HttpContext.Response.StatusCode = result.StatusCode ?? StatusCodes.Status400BadRequest;
    }
}