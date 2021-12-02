using System.Net;

using Library.Core;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Library.Service.Api;

public static class HelperExtensions
{
    public static IDictionary<string, string> ToErrorMap(this ModelStateDictionary state)
    {
        var errorList = new Dictionary<string, string>();
        if (state.IsValid)
        {
            return errorList;
        }

        foreach (string key in state.Keys)
        {
            var errors = state[key].Errors;
            if (errors?.Count == 0)
            {
                continue;
            }

            var message = errors.FirstOrDefault()?.ErrorMessage;
            if (!string.IsNullOrEmpty(message))
            {
                errorList.Add(key, errors.FirstOrDefault()?.ErrorMessage);
            }
        }

        return errorList;
    }

    public static ObjectResult AsObjectResult(this Exception ex)
    {
        var siteExc = new SiteException(ErrorCode.Default);
        int? code = null;

        // Check for exception type and map to inner exception
        if (ex is ApiSiteException appExc)
        {
            siteExc = appExc.SiteException;
            code = appExc.StatusCode;
        }

        // Check for normal site exception
        else if (ex is SiteException)
        {
            siteExc = ex as SiteException;
            if (siteExc.ErrorCode == ErrorCode.MissingEntity) code = StatusCodes.Status404NotFound;
        }

        return new ObjectResult(siteExc?.ToDto())
        {
            StatusCode = code
        };
    }

    public static bool TryGetHeader(this HttpContext context, string key, out string value)
    {
        value = string.Empty;
        if (context?.Request?.Headers == null)
        {
            return false;
        }

        if (context.Request.Headers.TryGetValue(key, out var v))
        {
            value = v;
        }

        return !string.IsNullOrEmpty(value);
    }

    public static bool IsCapacitor(this HttpContext context)
    {
        return context.Request.Headers.Origin == "capacitor://localhost";
    }

    public static bool IsPostman(this HttpContext context)
    {
        return context.TryGetHeader("Postman-Token", out _);
    }

    public static string UrlDecode(this string value)
    {
        return string.IsNullOrEmpty(value) ? value : WebUtility.UrlDecode(value).Replace("+", " ");
    }

    public static string[] UrlDecodeToArray(this string stringValues)
    {
        if (string.IsNullOrEmpty(stringValues))
        {
            return default;
        }

        var values = stringValues.Split(",");
        return values.Select(x => x.UrlDecode()).ToArray();
    }

    public static bool IsDevMode(this IWebHostEnvironment env) => env.EnvironmentName.ToLower() == "development";
}