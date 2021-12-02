using System.Net;

using Library.Core;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace Library.Service.Api;

public interface IReCaptchaService
{
    Task<ReCaptchaResponse> Verify(string key, IPAddress ipAddress = null);
}

public sealed class ReCaptchaAttribute : TypeFilterAttribute
{
    public ReCaptchaAttribute() : base(typeof(ReCaptchaFilter))
    {
    }
}

public class ReCaptchaFilter : IAsyncAuthorizationFilter
{
    private readonly IReCaptchaService _reCaptcha;
    private readonly bool _isDevMode;

    public ReCaptchaFilter(IReCaptchaService reCaptcha, IWebHostEnvironment env)
    {
        _reCaptcha = reCaptcha;
        _isDevMode = env.IsDevMode();
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if ((_isDevMode && context.HttpContext.IsPostman()) || context.HttpContext.IsCapacitor())
        {
            return;
        }

        try
        {
            // Find recaptcha token from header
            if (!context.HttpContext.TryGetHeader(StaticHeaders.RecaptchaToken, out var reCaptchaKey))
            {
                throw new Exception();
            }

            // Do not validate token in Dev Mode
            else if (_isDevMode)
            {
                return;
            }

            // Do we need recaptcha to pass?
            var response = await _reCaptcha.Verify(reCaptchaKey, context.HttpContext.Connection.RemoteIpAddress);
            if (response?.Success != true)
            {
                throw new Exception();
            }
        }
        catch
        {
            var siteEx = new ApiSiteException(StatusCodes.Status403Forbidden, new SiteException(ErrorCode.ReCaptchaFailed));
            context.Result = siteEx.AsObjectResult();
        }
    }
}

public class ReCaptchaResponse
{
    [JsonProperty("challenge_ts")]
    public DateTime ChallengeDate { get; set; }

    [JsonProperty("error-codes")]
    public IEnumerable<string> ErrorCodes { get; set; }

    [JsonProperty("hostname")]
    public string Hostname { get; set; }

    [JsonProperty("success")]
    public bool Success { get; set; }
}

public class ReCaptchaService : IReCaptchaService
{
    private readonly SharedSettings _settings;

    public ReCaptchaService(IOptions<SharedSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<ReCaptchaResponse> Verify(string key, IPAddress ipAddress = null)
    {
        using var client = new HttpClient();

        var pairs = new List<KeyValuePair<string, string>>
            {
                { new KeyValuePair<string, string>("secret", _settings.ReCaptchaSecretKey) },
                { new KeyValuePair<string, string>("response", key) },
                { new KeyValuePair<string, string>("remoteip", ipAddress?.ToString()) }
            };

        var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", new FormUrlEncodedContent(pairs));
        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<ReCaptchaResponse>(content);
    }
}