using System.Net;
using System.Reflection;
using System.Text;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace Comms.Core.SMS;

public class TextMagicSmsProvider : ISmsProvider
{
    private readonly string _username;
    private readonly string _token;

    public TextMagicSmsProvider(IOptions<TextMagicSettings> options)
    {
        _username = options.Value.Username;
        _token = options.Value.ApiKey;
    }

    public async Task<(bool sent, string error)> SendAsync(string number, string message)
    {
        using var client = new HttpClient();

        var assembly = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
        var useragent = $"textmagic-rest-csharp/{ assembly.Version } (.NET { Environment.Version }; { Environment.OSVersion })";

        client.BaseAddress = new Uri("https://rest.textmagic.com/api/v2/");

        client.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");
        client.DefaultRequestHeaders.Add("X-TM-Username", _username);
        client.DefaultRequestHeaders.Add("X-TM-Key", _token);

        var options = new
        {
            text = message,
            phones = number
        };

        var body = new StringContent(JsonConvert.SerializeObject(options), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("messages", body);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode >= HttpStatusCode.BadRequest)
            {
                try { content = JsonConvert.DeserializeObject<TextMagicException>(content)?.Message ?? response.ReasonPhrase; }
                catch { }
            }

            return (false, content);
        }

        return (true, null);
    }

    private class TextMagicException
    {
        public string Message { get; set; }
    }
}