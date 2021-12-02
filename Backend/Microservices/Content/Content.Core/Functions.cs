
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace Content.Core;

public static class Functions
{
    /// <summary>
    /// Returns a colour based on given rgb values
    /// </summary>
    /// <param name="r">"int": red value</param>
    /// <param name="g">"int": green value</param>
    /// <param name="b">"int": blue value</param>
    /// <returns>string</returns>
    public static string GetColourRange(int r, int g, int b)
    {
        // Black/White/Grey
        int top = ((r > g && r > b) ? r : (b > r && b > g ? b : g));
        int bottom = ((r < g && r < b) ? r : (b < r && b < g ? b : g));
        int dif = (top - bottom);

        if (r <= 50 && g <= 50 && b <= 50 && dif < 25)
            return "Black";

        if (r > 100 && r < 200 && g > 100 && g < 200 && b > 100 && b < 200 && dif < 25)
            return "Grey";

        if (r > 200 && g > 200 && b > 200 && dif < 45)
            return "White";

        // Orangey (>b>g, >100, <100)
        if (g > 100 && b < 100 && r > g && r > b && dif > 25)
            return "Orange";

        // Bluey (>150, >150, >r>g)
        if (r > 150 && g > 150 && b > r && b > g && dif > 25)
            return "Bluey";

        // Greeny (<200, <125, >r>b)
        if (r < 200 && b < 125 && g > r && g > b && dif > 25)
            return "Greeny";

        // Red (255, 0, 0)
        if (r > 150 && g < 150 && b < 150)
            return "Red";

        // Green (0, 255, 0)
        if (r < 150 && g > 150 && b < 150)
            return "Green";

        // Blue (0, 0, 255)
        if (r < 150 && g < 150 && b > 150)
            return "Blue";

        // Yellow (255, 255, 0)
        if (r > 150 && g > 150 && b < 150)
            return "Yellow";

        // Cyan (0, 255, 255)
        if (r < 150 && g > 150 && b > 150)
            return "Cyan";

        // Magenta (255, 0, 255)
        if (r > 150 && g < 150 && b > 150)
            return "Magenta";

        return "";
        //return $"{ r.ToString("X2") }{ g.ToString("X2") }{ b.ToString("X2") } ({ r }, { g }, { b })";
    }

    /// <summary>
    /// Extracts co-ordinates from a given ExifProfile
    /// </summary>
    /// <param name="profile">"ExifProfile": the data used to extract co-ordinates</param>
    /// <param name="lat">"decimal" [out]: the latitude</param>
    /// <param name="lng">"decimal" [out]: the longitude</param>
    public static (decimal lat, decimal lng) GetCoords(this ExifProfile profile)
    {
        decimal lat = 0;
        decimal lng = 0;

        // Ensure tags are all available
        var locationTags = new List<ExifTag>() { ExifTag.GPSLatitudeRef, ExifTag.GPSLatitude, ExifTag.GPSLongitudeRef, ExifTag.GPSLongitude };
        if (profile.Values.Count(x => locationTags.Contains(x.Tag)) != locationTags.Count)
        {
            return (lat, lng);
        }

        try
        {
            // Latitude
            var latitude = (Rational[])profile.Values.FirstOrDefault(x => x.Tag == ExifTag.GPSLatitude)?.GetValue();
            var @ref = profile.Values.FirstOrDefault(x => x.Tag == ExifTag.GPSLatitudeRef)?.GetValue().ToString();

            var degrees = (decimal)latitude[0].ToDouble();
            var minutes = (decimal)latitude[1].ToDouble();

            var second_parts = latitude[2].ToString().Split('/');
            var seconds = decimal.Parse(second_parts[0]) / decimal.Parse(second_parts[1]);

            lat = degrees + (((minutes * 60) + seconds) / 3600);
            if (@ref == "S" || @ref == "W")
            {
                lat = -lat;
            }

            // Longitude
            var longitude = (Rational[])profile.Values.FirstOrDefault(x => x.Tag == ExifTag.GPSLongitude)?.GetValue();
            @ref = profile.Values.FirstOrDefault(x => x.Tag == ExifTag.GPSLongitudeRef)?.GetValue().ToString();

            degrees = (decimal)longitude[0].ToDouble();
            minutes = (decimal)longitude[1].ToDouble();

            second_parts = longitude[2].ToString().Split('/');
            seconds = decimal.Parse(second_parts[0]) / decimal.Parse(second_parts[1]);

            lng = degrees + (((minutes * 60) + seconds) / 3600);
            if (@ref == "S" || @ref == "W")
            {
                lng = -lng;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return (lat, lng);
    }

    /// <summary>
    /// Removes the coords from the Exif Profile.
    /// </summary>
    /// <param name="profile">The profile.</param>
    /// <returns></returns>
    public static void RemoveCoords(this ExifProfile profile)
    {
        if (profile == null)
        {
            return;
        }

        profile.RemoveValue(ExifTag.GPSAltitude);
        profile.RemoveValue(ExifTag.GPSAltitudeRef);
        profile.RemoveValue(ExifTag.GPSLongitude);
        profile.RemoveValue(ExifTag.GPSLongitudeRef);
        profile.RemoveValue(ExifTag.GPSDestLongitude);
        profile.RemoveValue(ExifTag.GPSDestLongitudeRef);
    }
}