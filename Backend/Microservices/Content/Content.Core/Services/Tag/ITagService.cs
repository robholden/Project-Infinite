
using Content.Domain;

namespace Content.Core.Services;

public interface ITagService
{
    Task Create(IEnumerable<Tag> tags);

    Task Update(IEnumerable<Tag> tags);

    Task Delete(IEnumerable<int> tags);

    Task Delete(int id);
}