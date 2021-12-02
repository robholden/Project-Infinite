namespace Comms.Core;

public class EmailSettings
{
    public string WebUrl { get; set; }

    public string Name { get; set; }

    public string EmailAddress { get; set; }

    public string Host { get; set; }

    public string Password { get; set; }

    public int Port { get; set; }

    public bool UseCredentials { get; set; }

    public string Username { get; set; }

    public bool UseSSL { get; set; }
}

public class TextMagicSettings
{
    public string Username { get; set; }

    public string ApiKey { get; set; }
}