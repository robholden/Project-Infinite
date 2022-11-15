
using Content.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;

namespace Content.Core.Services;

public class TagService : ITagService
{
    private readonly ContentContext _ctx;

    public TagService(ContentContext ctx)
    {
        _ctx = ctx;
    }

    public Task Create(IEnumerable<Tag> tags) => _ctx.CreateManyAsync(tags);

    public async Task Update(IEnumerable<Tag> tags)
    {
        // Find existing tags
        var tagMap = tags.ToDictionary(t => t.Id, t => t);

        // Update tags
        await _ctx.Tags.Where(x => tagMap.ContainsKey(x.Id)).ExecuteUpdateAsync(prop => prop
            .SetProperty(p => p.Value, p => tagMap[p.Id].Value)
            .SetProperty(p => p.Weight, p => tagMap[p.Id].Weight)
        );
    }

    public async Task Delete(IEnumerable<int> tags)
    {
        await _ctx.Tags.Where(x => tags.Contains(x.Id)).ExecuteDeleteAsync();
    }

    public async Task Delete(int id)
    {
        await _ctx.Tags.Where(t => t.Id == id).ExecuteDeleteAsync();
    }
}