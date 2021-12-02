
using AutoMapper;

using Content.Api.Dtos;
using Content.Api.Results;
using Content.Domain;

using Library.Core;
using Library.Service.Api;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Content.Api.Controllers;

public class FiltersController : BaseController<FiltersController>
{
    private readonly ContentContext _ctx;
    private readonly IWebHostEnvironment _env;

    public FiltersController(ILogger<FiltersController> logger, IMapper mapper, ContentContext ctx, IWebHostEnvironment env)
        : base(logger, mapper)
    {
        _ctx = ctx;
        _env = env;
    }

    [HttpGet]
    public async Task<FiltersResult> Get([FromServices] IMemoryCache cache)
    {
        // Check cache
        var cacheKey = "content_filters";
        if (!_env.IsDevMode() && cache.TryGetValue<FiltersResult>(cacheKey, out var filters))
        {
            return filters;
        }

        // Get tags
        var tags = await _ctx.Tags
            .Include(l => l.Pictures)
            .Where(l => l.Pictures.Any(p => p.Status == PictureStatus.Published))
            .OrderBy(t => t.Value)
            .ToListAsync();
        var tagDtos = _mapper.Map<IEnumerable<TagDto>>(tags);

        // Get locations
        var locations = await _ctx.Locations
            .Include(l => l.Country)
            .Include(l => l.Pictures)
            .Where(l => l.Pictures.Any(p => p.Status == PictureStatus.Published))
            .ToListAsync();
        var locationDtos = _mapper.Map<IEnumerable<LocationDto>>(locations);

        // Cache filters
        filters = new FiltersResult(locationDtos, tagDtos);
        cache.Set(cacheKey, filters, DateTime.UtcNow.AddMinutes(5));

        return filters;
    }
}