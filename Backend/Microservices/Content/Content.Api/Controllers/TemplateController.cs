using System.Net;

using AutoMapper;

using Library.Service.Api;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Content.Api.Controllers;

public class TemplateController : BaseController<TemplateController>
{
    private readonly IMemoryCache _cache;

    public TemplateController(ILogger<TemplateController> logger, IMapper mapper, IMemoryCache cache)
        : base(logger, mapper)
    {
        _cache = cache;
    }

    [HttpGet("{type}/{name}")]
    public ContentResult GetTemplate([FromRoute] string type, [FromRoute] string name)
    {
        var key = $"{ type }_{ name }";
        if (!_cache.TryGetValue(key, out string html))
        {
            ThrowNotFound();
        }

        return new ContentResult
        {
            ContentType = "text/html",
            StatusCode = (int)HttpStatusCode.OK,
            Content = html
        };
    }
}