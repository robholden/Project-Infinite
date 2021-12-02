using System.ComponentModel.DataAnnotations;

namespace Content.Domain;

public class Coords
{
    public Coords()
    {
    }

    public Coords(decimal lat, decimal lng)
    {
        Lat = lat;
        Lng = lng;
    }

    [Required]
    public decimal Lat { get; set; }

    [Required]
    public decimal Lng { get; set; }

    public bool InBounds(Boundry boundry)
    {
        return Lat >= boundry.MinLat && Lat <= boundry.MaxLat && Lng >= boundry.MinLng && Lng <= boundry.MaxLng;
    }
}