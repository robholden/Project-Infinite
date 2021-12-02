
using Content.Domain;

using Microsoft.EntityFrameworkCore;

namespace Content.Core.Queries;

public class TagQueries : ITagQueries
{
    private readonly ContentContext _ctx;

    public TagQueries(ContentContext context)
    {
        _ctx = context;
    }

    public IEnumerable<Tag> GetAll()
    {
        return _ctx.Tags
            .AsNoTracking()
            .OrderBy(x => x.Weight);
    }
}