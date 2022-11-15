
using Content.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;

namespace Content.Core.Services;

public class CollectionService : ICollectionService
{
    private readonly ContentContext _ctx;

    public CollectionService(ContentContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<Guid> Create(IUser user, string name)
    {
        var collection = new Collection
        {
            UserId = user.UserId,
            Username = user.Username,
            Name = name
        };
        collection = await _ctx.CreateAsync(collection);

        return collection.CollectionId;
    }

    public async Task AddOrRemovePicture(Guid collectionId, Guid pictureId)
    {
        var collection = await _ctx.Collections
            .Include(c => c.Pictures.Where(x => x.PictureId == pictureId))
            .FindAsync(c => c.CollectionId == collectionId);

        if (collection.Pictures.Count > 0)
        {
            collection.Pictures.Remove(collection.Pictures.First());
        }
        else
        {
            var picture = await _ctx.Pictures.FindAsync(p => p.PictureId == pictureId && (p.Status == PictureStatus.Published || p.UserId == collection.UserId));
            collection.Pictures.Add(picture);
        }

        await _ctx.SaveChangesAsync();
    }

    public async Task Update(Guid collectionId, string name)
    {
        var collection = await _ctx.Collections.FindAsync(c => c.CollectionId == collectionId);

        collection.Name = name;

        await _ctx.UpdateAsync(collection);
    }

    public async Task Delete(Guid collectionId)
    {
        var collection = await _ctx.Collections.FindOrNullAsync(c => c.CollectionId == collectionId);
        if (collection == null)
        {
            return;
        }

        await _ctx.RemoveAsync(collection);
    }
}