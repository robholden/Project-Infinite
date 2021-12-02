
using Content.Core.Services;
using Content.Domain;

using Library.Core;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Content.Core.BackgroundTasks;

public class CreateLocationsTask : BackgroundTask<CreateLocationsTask>
{
    private static readonly Guid _ownerId = Guid.NewGuid();
    private readonly IServiceScopeFactory _scopeFactory;
    private bool _running;

    public CreateLocationsTask(ILogger<CreateLocationsTask> logger, IServiceScopeFactory scopeFactory) : base(logger, 15)
    {
        _scopeFactory = scopeFactory;
    }

    public override async void DoWork(object state)
    {
        if (_running)
        {
            return;
        }

        _running = true;
        try
        {
            using var scope = _scopeFactory.CreateScope();

            // Get database context
            var ctx = scope.ServiceProvider.GetRequiredService<ContentContext>();

            // Find pictures that are missing locations
            var requests = await ctx.PictureLocationRequests
                .Where(x => !x.OwnedBy.HasValue || x.OwnedBy == _ownerId)
                .OrderBy(x => x.Date)
                .Take(10)
                .ToListAsync();

            // Stop if empty
            if (requests.Count == 0)
            {
                return;
            }

            // Assign this batch an owner id
            requests.ForEach(x => x.OwnedBy = _ownerId);
            try
            {
                ctx.UpdateRange(requests);
                await ctx.SaveChangesAsync();

                Console.WriteLine($"{ _ownerId } now owns { requests.Count } requests");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ _ownerId } failed to own requests: { ex.Message }");
                return;
            }

            // Add each location and assign to pictures
            var service = scope.ServiceProvider.GetRequiredService<ILocationService>();
            List<Location> added = new();
            foreach (var request in requests)
            {
                // Check if we've added a location within our bounds
                if (added.Any(x => request.InBounds(x.Boundry))) continue;

                // Call api and create location
                try
                {
                    var location = await service.AddWithCoords(request.Lat, request.Lng);
                    added.Add(location);

                    // Delete this request's entry
                    ctx.Remove(request);
                    await ctx.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed calling server for location");
                    request.OwnedBy = null;
                    ctx.Update(request);
                    await ctx.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add missing picture locations");
        }
        finally
        {
            _running = false;
        }
    }
}