using System.Linq.Expressions;

using Content.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;

namespace Content.Core.Queries;

public class PictureModerationQueries : IPictureModerationQueries
{
    private readonly ContentContext _ctx;

    public PictureModerationQueries(ContentContext context)
    {
        _ctx = context;
    }

    public async Task<PagedList<PictureModeration>> Lookup(string username, IPagedListRequest<PictureModerationQueryOptions> pageRequest)
    {
        // Default options
        var options = pageRequest.Options ?? new PictureModerationQueryOptions();

        // Build where clause
        Expression<Func<PictureModeration, bool>> where = _ => true;

        // Name
        if (!string.IsNullOrEmpty(options.Username))
        {
            where = where.And(x => x.Picture.Username.ToLower().StartsWith(options.Username.ToLower().Trim()));
        }

        // Return in given order
        Expression<Func<PictureModeration, object>> orderBy = null;
        switch (options.OrderBy)
        {
            case PictureModerationQueryOptions.OrderByEnum.Date:
                orderBy = x => x.Date;
                break;

            case PictureModerationQueryOptions.OrderByEnum.Username:
                orderBy = x => x.Picture.Username;
                break;
        }

        return await _ctx.PictureModerations
            .AsNoTracking()
            .Include(x => x.Picture.Tags)
            .Include(x => x.Picture).ThenInclude(p => p.Location).ThenInclude(l => l.Country)
            .OrderBy(x => x.LockedBy == username)
            .QueryAsync(where, pageRequest, orderBy);
    }
}