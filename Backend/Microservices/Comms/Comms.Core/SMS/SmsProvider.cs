namespace Comms.Core.SMS;

public interface ISmsProvider
{
    Task<(bool sent, string error)> SendAsync(string number, string message);
}