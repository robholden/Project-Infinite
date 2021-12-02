
using Library.Core.Models;

namespace Content.Core.Services;

public interface ICollectionService
{
    Task<Guid> Create(IUser user, string name);

    Task AddOrRemovePicture(Guid collectionId, Guid pictureId);

    Task Update(Guid collectionId, string name);

    Task Delete(Guid collectionId);
}