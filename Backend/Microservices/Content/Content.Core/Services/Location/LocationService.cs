
using Content.Domain;

using Library.Core;
using Library.Service.PubSub;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace Content.Core.Services;

public class LocationService : ILocationService
{
    private readonly ContentContext _ctx;
    private readonly ContentSettings _settings;

    private readonly ISocketsPubSub _socketEvents;

    public LocationService(ContentContext ctx, IOptions<ContentSettings> options, ISocketsPubSub socketEvents)
    {
        _ctx = ctx;
        _settings = options.Value;

        _socketEvents = socketEvents;
    }

    public async Task UpdateName(Guid id, string name)
    {
        var location = await _ctx.Locations.FindAsync(x => x.LocationId == id);

        location.Name = name;

        await _ctx.UpdateAsync(location);
    }

    public async Task UpdateCode(Guid id, string code)
    {
        var location = await _ctx.Locations.FindAsync(x => x.LocationId == id);

        location.Code = code;

        await _ctx.UpdateAsync(location);
    }

    public async Task UpdateBoundry(Guid locationId, Boundry newBoundry)
    {
        var boundry = await _ctx.Boundries.FindAsync(x => x.LocationId == locationId);

        boundry.MinLat = newBoundry.MinLat;
        boundry.MinLng = newBoundry.MinLng;
        boundry.MaxLat = newBoundry.MaxLat;
        boundry.MaxLng = newBoundry.MaxLng;

        await _ctx.UpdateAsync(boundry);
    }

    public async Task<Location> AddWithCoords(decimal lat, decimal lng)
    {
        // Call LocationIQ api for location data
        var url = "https://eu1.locationiq.com/v1/reverse.php?format=json&normalizecity=1&zoom=10&lat={0}&lon={1}&key={2}";
        var uri = new Uri(string.Format(url, lat, lng, _settings.LocationIQToken));
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);

        using var client = new HttpClient() { BaseAddress = uri };

        var httpResult = await client.SendAsync(httpRequest);
        httpResult.EnsureSuccessStatusCode();

        var data = await httpResult.Content.ReadAsStringAsync();
        if (data == null)
        {
            throw new SiteException(ErrorCode.FailedToLookupCoords);
        }

        var map = JsonConvert.DeserializeObject<LocationIQResponse>(data);
        if (map == null)
        {
            throw new SiteException(ErrorCode.FailedToDecodeJson);
        }

        var city = map.Address?.City ?? map.Address?.State;
        if (string.IsNullOrEmpty(city))
        {
            throw new SiteException(ErrorCode.FailedToLookupCoords);
        }

        var cc = map.Address.CountryCode.ToUpper();
        var country = await _ctx.Countries.FindAsync(c => c.Code == cc, ErrorCode.FailedToGetCountry);
        var boundry = new Boundry
        {
            MinLat = map.BoundingBox[0],
            MaxLat = map.BoundingBox[1],
            MinLng = map.BoundingBox[2],
            MaxLng = map.BoundingBox[3]
        };

        // Check for an existing location within these boundries
        var location = await _ctx.Locations
            .Include(l => l.Boundry)
            .FirstOrDefaultAsync(l => l.Boundry.Equals(boundry));

        // Add new location do database
        if (location == null)
        {
            location = new Location
            {
                Name = city,
                Code = city,
                Country = country,
                Lat = map.Lat,
                Lng = map.Lon,
                Boundry = boundry
            };
            location = await _ctx.CreateAsync(location);
        }

        // Update all picture in range of this location
        var pictures = (await _ctx.Pictures
            .Where(x => x.Lat >= boundry.MinLat && x.Lat <= boundry.MaxLat && x.Lng >= boundry.MinLng && x.Lng <= boundry.MaxLng)
            .ToListAsync())
            .Select(x => { x.Location = location; return x; });

        await _ctx.UpdateManyAsync(pictures);
        SendLocationToClients(pictures);

        return location;
    }

    private void SendLocationToClients(IEnumerable<Picture> pictures)
    {
        // Send socket message to all pictures waiting for their picture location
        var tasks = pictures.Select(async x => await _socketEvents?.NewLocation(x.UserId, x.PictureId, x.Location.Name, x.Location.Country.Name, x.Location.Lat, x.Location.Lng));

        // Execute tasks in the background
        _ = Task.Factory.StartNew(async () => await Task.WhenAll(tasks), TaskCreationOptions.LongRunning);
    }

    private class LocationIQResponse
    {
        public decimal Lat { get; set; }

        public decimal Lon { get; set; }

        public AddressResponse Address { get; set; }

        public decimal[] BoundingBox { get; set; }

        public class AddressResponse
        {
            public string State { get; set; }

            public string City { get; set; }

            [JsonProperty("country_code")]
            public string CountryCode { get; set; }
        }
    }
}