using Library.Service.PubSub;

namespace Identity.Core;

public record UserFieldChange(string Property, object Value);

public static class SocketMethods
{
    public static Task NewSession(this ISocketsPubSub sub, Guid userId) => sub.Send(new("SessionAdded", userId));

    public static Task InvalidateUsers(this ISocketsPubSub sub) => sub.Send(new("InvalidateUser"));

    public static Task RevokeSession(this ISocketsPubSub sub, Guid userId, Guid? authTokenId = null) => sub.Send(new("SessionRevoked", userId, authTokenId));

    public static Task UpdatedUserFields(this ISocketsPubSub sub, Guid userId, params UserFieldChange[] changes) => sub.Send(new("UpdatedUserFields", userId, changes));
}
