using System.Linq.Expressions;

using Content.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;

namespace Content.Core.Queries;

public class LocationQueries : ILocationQueries
{
    private readonly ContentContext _ctx;

    public LocationQueries(ContentContext context)
    {
        _ctx = context;
    }

    public IEnumerable<Location> GetAll()
    {
        return _ctx.Locations
            .AsNoTracking()
            .Include(x => x.Boundry)
            .Include(x => x.Country)
            .OrderBy(x => x.Country.Name)
            .OrderBy(x => x.Name);
    }

    public async Task<Location> WithinBounds(decimal lat, decimal lng)
    {
        return await _ctx.Locations
                .Include(x => x.Boundry)
                .Include(x => x.Country)
                .FindOrNullAsync(x => lat >= x.Boundry.MinLat && lat <= x.Boundry.MaxLat && lng >= x.Boundry.MinLng && lng <= x.Boundry.MaxLng);
    }

    public async Task<PagedList<Location>> Lookup(IPagedListRequest<LocationQueryOptions> pageRequest)
    {
        // Default options
        var options = pageRequest.Options ?? new LocationQueryOptions();

        // Build where clause
        Expression<Func<Location, bool>> where = _ => true;

        // Name
        if (!string.IsNullOrEmpty(options.Name))
        {
            where = where.And(x => x.Name.ToLower().StartsWith(options.Name.ToLower().Trim()));
        }

        // Return in given order
        Expression<Func<Location, object>> orderBy = null;
        switch (options.OrderBy)
        {
            case LocationQueryOptions.OrderByEnum.Name:
                orderBy = x => x.Name;
                break;
        }

        return await _ctx.Locations
            .AsNoTracking()
            .Include(x => x.Boundry)
            .Include(x => x.Country)
            .OrderBy(x => x.Country.Name)
            .QueryAsync(where, pageRequest, orderBy);
    }
}