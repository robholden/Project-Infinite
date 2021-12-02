
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

    public Task Create(IEnumerable<Tag> tags) => _ctx.PostRange(tags);

    public async Task Update(IEnumerable<Tag> tags)
    {
        // Find existing tags
        var tagIds = tags.Select(t => t.Id);
        var entities = await _ctx.Tags.Where(x => tagIds.Contains(x.Id)).ToListAsync();

        // Update and save
        var updated = entities.Select(tag =>
        {
            var t = tags.FirstOrDefault(x => x.Id == x.Id);
            tag.Value = t.Value;
            tag.Weight = t.Weight;
            return tag;
        });

        await _ctx.PutRange(updated);
    }

    public async Task Delete(IEnumerable<int> tags)
    {
        var tag = _ctx.Tags.Where(x => tags.Any(id => id == x.Id));
        await _ctx.DeleteRange(tag);
    }

    public async Task Delete(int id)
    {
        var tag = await _ctx.Tags.FindAsync(t => t.Id == id);
        await _ctx.Delete(tag);
    }
}