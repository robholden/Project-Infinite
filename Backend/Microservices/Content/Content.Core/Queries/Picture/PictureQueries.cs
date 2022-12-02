using System.Linq.Expressions;

using Content.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Content.Core.Queries;

public class PictureQueries : IPictureQueries
{
    private readonly ContentContext _ctx;
    private readonly IMemoryCache _cache;

    public PictureQueries(ContentContext context, IMemoryCache cache)
    {
        _ctx = context;
        _cache = cache;
    }

    public async Task<Picture> Get(Guid pictureId, Guid? userId)
    {
        // Build express for picture
        IQueryable<Picture> query = _ctx.Pictures
                .AsNoTracking()
                .Include(x => x.Location).ThenInclude(x => x.Country)
                .Include(x => x.Tags);

        // Include user specific data
        if (userId.HasValue)
        {
            query = query
                .Include(x => x.Collections.Where(c => c.UserId == userId))
                .Include(x => x.Likes.Where(l => l.UserId == userId));
        }

        // Lookup picture and ensure it exists
        var picture = await query.FirstOrDefaultAsync(x => x.PictureId == pictureId);
        if (picture == null)
        {
            return null;
        }

        // Assign metadata to picture
        picture.LikesTotal = await _ctx.PictureLikes.CountAsync(x => x.PictureId == pictureId);
        return picture.Seeded();
    }

    public Task<bool> IsAccessible(Guid pictureId)
    {
        return _ctx.Pictures.AsNoTracking().AnyAsync(x => x.PictureId == pictureId && x.Status == PictureStatus.Published);
    }

    public Task<bool> BelongsToUser(Guid userId, Guid pictureId)
    {
        return _ctx.Pictures.AsNoTracking().AnyAsync(x => x.PictureId == pictureId && x.UserId == userId);
    }

    public Task<bool> BelongsToUser(Guid userId, Guid[] pictureIds)
    {
        return _ctx.Pictures.AsNoTracking().AnyAsync(x => pictureIds.Contains(x.PictureId) && x.UserId == userId);
    }

    public async Task<IEnumerable<Picture>> NearBy(Guid pictureId)
    {
        var picture = await _ctx.Pictures.FindAsync(x => x.PictureId == pictureId);

        return await _ctx.Pictures
            .AsNoTracking()
            .Include(x => x.Location).ThenInclude(x => x.Country)
            .Where(p => p.PictureId != picture.PictureId && p.Status == PictureStatus.Published)
            .Where(picture.IsWithinDistance<Picture>(50))
            .OrderByDescending(x => x.CreatedDate)
            .Take(10)
            .ToListAsync();
    }

    public async Task<IEnumerable<Picture>> Matches(Guid pictureId, int limit = 20)
    {
        var colours = (await _ctx.Pictures.FindAsync(p => p.PictureId == pictureId))?.Colours;
        return _ctx.Pictures
            .AsNoTracking()
            .Include(x => x.Tags)
            .Include(x => x.Location).ThenInclude(x => x.Country)
            .Where(p => p.Colours == colours && p.PictureId != pictureId && p.Status == PictureStatus.Published)
            .Take(limit);
    }

    public async Task<PagedList<Picture>> Featured(int page = 1, int pageSize = 10)
    {
        // Get current seed
        var seedKey = "content_featured_seed";
        if (_cache.TryGetValue(seedKey, out string seed) && _cache.TryGetValue<PagedList<Picture>>($"{seed}_{page}_{pageSize}", out var results))
        {
            return results;
        }

        // Generate new seed and cache result
        seed = Guid.NewGuid().ToString();
        _cache.Set(seedKey, seed, DateTime.UtcNow.AddMinutes(5));

        // Build lookup options
        var options = new PictureQueryOptions() { Seed = seed, Status = PictureStatus.Published };

        // Create lookup request
        var request = new PagedListRequest<PictureQueryOptions>(page, pageSize, null, options);
        var pictures = await Lookup(request);

        // Cache when results are more than 25
        if (pictures.TotalRows > 25)
        {
            _cache.Set($"{seed}_{page}_{pageSize}", pictures, DateTime.UtcNow.AddMinutes(60));
        }

        return pictures;
    }

    public IDictionary<Guid, bool> LikesMap(IEnumerable<Guid> pictureIds, Guid userId)
    {
        var results = _ctx.Pictures
            .AsNoTracking()
            .Include(x => x.Likes)
            .Where(p => p.Likes.Any(l => l.UserId == userId) && pictureIds.Contains(p.PictureId));

        return pictureIds.ToDictionary(key => key, value => results.Any(r => r.PictureId == value));
    }

    public Task<bool> Likes(Guid pictureId, Guid userId)
    {
        return _ctx.Pictures
             .AsNoTracking()
             .Include(x => x.Likes)
             .AnyAsync(x => x.Likes.Any(l => l.UserId == userId) && x.PictureId == pictureId);
    }

    public async Task<PagedList<Picture>> Lookup(IPagedListRequest<PictureQueryOptions> pageRequest)
    {
        // Build where clause
        var where = BuildExpression(pageRequest.Options, out var orderBy);

        return await _ctx.Pictures
            .AsNoTracking()
            .Include(x => x.Tags)
            .Include(x => x.Location).ThenInclude(x => x.Country)
            .QueryAsync(where, pageRequest, orderBy);
    }

    public IEnumerable<Picture> Search(PictureQueryOptions options, OrderByDirection orderDir = OrderByDirection.Desc)
    {
        // Build where clause
        var where = BuildExpression(options, out var orderBy);

        var enumer = _ctx.Pictures
            .AsNoTracking()
            .Include(x => x.Tags)
            .Include(x => x.Location).ThenInclude(x => x.Country)
            .Where(where);

        if (orderBy == null) return enumer;
        else if (orderDir == OrderByDirection.Asc) return enumer.OrderBy(orderBy);
        else return enumer.OrderByDescending(orderBy);
    }

    public Task<int> Count(PictureQueryOptions options)
    {
        // Build where clause
        var where = BuildExpression(options, out _);

        return _ctx.Pictures
            .AsNoTracking()
            .Include(x => x.Location)
            .CountAsync(where);
    }

    private static Expression<Func<Picture, bool>> BuildExpression(PictureQueryOptions options, out Expression<Func<Picture, object>> orderBy)
    {
        // Default options
        options ??= new PictureQueryOptions();

        // Build where clause
        orderBy = null;
        Expression<Func<Picture, bool>> where = p => options.Draft ? p.Status == PictureStatus.Draft : p.Status != PictureStatus.Draft;

        // Name
        if (!string.IsNullOrEmpty(options.Name))
        {
            where = where.And(x => x.Name.ToLower().StartsWith(options.Name.ToLower().Trim()));
        }

        // Only show status
        if (options.Status.HasValue)
        {
            where = where.And(x => x.Status == options.Status);
        }

        // Location and distance
        if (options.Locations?.Any() == true)
        {
            where = where.And(x => options.Locations.Contains(x.Location.Code));
            if (options.Distance.HasValue && options.Locations.Length == 1) where = where.And(options.Distance.Value.IsWithinDistance());
        }

        // Country
        else if (options.Countries?.Any() == true)
        {
            where = where.And(x => options.Countries.Contains(x.Location.Country.Code));
        }

        // Tags
        if (options.Tags?.Any() == true)
        {
            where = where.And(x => x.Tags.Any(t => options.Tags.Contains(t.Value)));
        }

        // Liked pictures for user
        if (options.ShowLikes && !string.IsNullOrEmpty(options.Username))
        {
            where = where.And(x => x.Likes.Any(l => l.Username == options.Username));
        }
        else if (!string.IsNullOrEmpty(options.Username))
        {
            where = where.And(x => x.Username == options.Username);
        }

        // Collection pictures
        if (options.CollectionId.HasValue)
        {
            where = where.And(x => x.Collections.Any(l => l.CollectionId == options.CollectionId.Value));
        }

        switch (options.OrderBy)
        {
            case PictureQueryOptions.OrderByEnum.Name:
                orderBy = x => x.Name;
                break;

            case PictureQueryOptions.OrderByEnum.UploadDate:
                orderBy = x => x.CreatedDate;
                break;

            case PictureQueryOptions.OrderByEnum.DateTaken:
                orderBy = x => x.DateTaken;
                break;
        }

        // Order by seed?
        if (!string.IsNullOrEmpty(options.Seed))
        {
            orderBy = _ => options.Seed;
        }

        return where;
    }
}