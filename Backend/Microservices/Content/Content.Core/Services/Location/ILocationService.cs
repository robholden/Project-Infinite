
using Content.Domain;

namespace Content.Core.Services;

public interface ILocationService
{
    Task<Location> AddWithCoords(decimal lat, decimal lng);

    Task UpdateName(Guid id, string name);

    Task UpdateCode(Guid id, string code);

    Task UpdateBoundry(Guid locationId, Boundry newBoundry);
}