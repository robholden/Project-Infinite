using System.Linq.Expressions;

using Content.Domain;

namespace Content.Core;

public static class ExtensionMethods
{
    public static Expression<Func<Picture, bool>> IsWithinDistance(this int distance)
    {
        return p =>
            (p.Lat > 0 || p.Lng > 0) &&
            Math.Sqrt(
                Math.Pow((double)(69.1m * (p.Location.Lat - p.Lat)), 2) +
                Math.Pow((double)(69.1m * (p.Lng - p.Location.Lng) * (decimal)Math.Cos((double)p.Location.Lat * 57.3d)), 2)
            ) <= distance;
    }

    public static Expression<Func<T, bool>> IsWithinDistance<T>(this Coords src, int distance) where T : Coords
    {
        return dest =>
            (src.Lat > 0 || src.Lng > 0) &&
            (dest.Lat > 0 || dest.Lng > 0) &&
            Math.Sqrt(
                Math.Pow((double)(69.1m * (src.Lat - dest.Lat)), 2) +
                Math.Pow((double)(69.1m * (dest.Lng - src.Lng) * (decimal)Math.Cos((double)src.Lat * 57.3d)), 2)
            ) <= distance;
    }
}