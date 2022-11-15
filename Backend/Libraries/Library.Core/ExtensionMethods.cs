using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using EnumsNET;

namespace Library.Core;

public static class ClaimsExtensions
{
    public static string GetClaim(this ClaimsPrincipal principal, string type)
    {
        return principal?.Claims?.FirstOrDefault(c => c.Type == type)?.Value;
    }

    public static T GetClaim<T>(this ClaimsPrincipal principal, string type)
    {
        var value = GetClaim(principal, type);
        if (string.IsNullOrEmpty(value))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value);
    }

    public static Guid? GetGuidClaim(this ClaimsPrincipal principal, string type)
    {
        if (Guid.TryParse(principal.GetClaim(type), out Guid userId))
        {
            return userId;
        }

        return null;
    }
}

public static class ObjectExtensions
{

}

public static class DictionaryExtensions
{
    public static string ToQueryString(this IDictionary<string, string> pairs, string prefix = "")
    {
        if (pairs?.Count == 0) return string.Empty;

        return prefix + string.Join("&", pairs.Select(p => p.Key + "=" + p.Value));
    }
}

public static class IntExtensions
{
    private static readonly Random _random = new();

    public static string ToRandom(this int @length)
    {
        var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, @length).Select(s => s[_random.Next(s.Length)]).ToArray());
    }

    public static bool WithinRange(this decimal src, decimal min, decimal max)
    {
        return src >= min && src <= max;
    }

    public static T ToEnum<T>(this string value, T def, bool ignoreCase = true)
    {
        if (string.IsNullOrEmpty(value))
        {
            return def;
        }

        try
        {
            value = value.Replace("_", "").Replace(" ", "");
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }
        catch
        {
            return def;
        }
    }
}

public static partial class StringExtensions
{
    public static bool IsTrue(this string value) => value?.ToLower() == "true" || value == "1";

    public static bool AreEqual(this string value, string toEqual) => value?.ToLower() == toEqual?.ToLower();

    public static bool IsValidEmail(this string email)
    {
        try
        {
            System.Net.Mail.MailAddress addr = new(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidUsername(this string username) => UsernameRegex().IsMatch(username);

    public static string ToHash(this string value, string salt = "")
    {
        // Create a SHA256
        using SHA256 hash = SHA256.Create();

        // ComputeHash - returns byte array
        byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(value + salt));

        // Convert byte array to a string
        StringBuilder builder = new();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }

        return builder.ToString();
    }

    [GeneratedRegex("^[a-zA-Z0-9][a-zA-Z0-9_]*$")]
    public static partial Regex UsernameRegex();
}

public static class DateExtensions
{
    public static DateTime ResetToMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    public static DateTime ResetToDay(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day);
    }

    public static DateTime ResetToHour(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
    }

    public static DateTime ResetToMinute(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
    }
}

public static class EnumExtensions
{
    public static string Description<TEnum>(this TEnum enumValue) where TEnum : struct, Enum
    {
        return enumValue.AsString(EnumFormat.Description);
    }
}

public static class TaskExtensions
{
    public static void FireAndForget(this Task task) => _ = Task.Run(async () => await task);

    public static bool TryRunTask<T>(this Task<T> fn, out T result, out Exception error)
    {
        error = null;
        result = default;
        try
        {
            result = fn.Result;
            return true;
        }
        catch (Exception ex)
        {
            error = ex;
            return false;
        }
    }

    public static bool TryRunTask(this Task fn, out Exception error)
    {
        error = null;
        try
        {
            fn.Wait();
            return true;
        }
        catch (Exception ex)
        {
            error = ex;
            return false;
        }
    }
}

public static class HttpClientExtensions
{
    public static async Task<T> GetAsync<T>(this HttpClient http, string url)
    {
        var response = await http.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<T>(await response.Content.ReadAsStreamAsync());
    }
}

public static class ByteExtensions
{
    public static string ByteToHex(this byte[] buff)
    {
        var sbinary = "";
        for (int i = 0; i < buff.Length; i++)
        {
            sbinary += buff[i].ToString("x2");
        }
        return sbinary;
    }

    public static string HashHmacSha256(this string message, string key)
    {
        var encoding = Encoding.UTF8;
        var keyByte = encoding.GetBytes(key);
        var hmac = new HMACSHA256(keyByte);
        var messageBytes = encoding.GetBytes(message);
        var hashmessage = hmac.ComputeHash(messageBytes);

        return hashmessage.ByteToHex();
    }
}