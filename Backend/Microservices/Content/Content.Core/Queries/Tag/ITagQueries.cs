
using Content.Domain;

namespace Content.Core.Queries;

public interface ITagQueries
{
    IEnumerable<Tag> GetAll();
}