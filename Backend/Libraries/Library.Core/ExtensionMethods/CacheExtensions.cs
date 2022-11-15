using System.Text.Json;

using Microsoft.Extensions.Caching.Distributed;

namespace Library.Core;

public static class CacheExtensions
{
    public static Task Set<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
    {
        return cache.SetStringAsync(key, JsonSerializer.Serialize(value), options);
    }

    public static Task Set<T>(this IDistributedCache cache, string key, T value)
    {
        return cache.SetStringAsync(key, JsonSerializer.Serialize(value));
    }

    public static async Task<T> Get<T>(this IDistributedCache cache, string key)
    {
        var value = await cache.GetStringAsync(key);
        if (value == null) return default;

        try
        {
            return JsonSerializer.Deserialize<T>(value);
        }
        catch
        {
            return default;
        }
    }
}