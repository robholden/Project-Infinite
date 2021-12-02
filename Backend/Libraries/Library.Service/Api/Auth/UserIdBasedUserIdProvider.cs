using Microsoft.AspNetCore.SignalR;

namespace Library.Service.Api.Auth;

public class UserIdBasedUserIdProvider : IUserIdProvider
{
    public virtual string GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst(UserClaimsKeys.UserIdentifier)?.Value;
    }
}