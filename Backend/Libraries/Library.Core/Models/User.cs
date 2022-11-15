namespace Library.Core;

public record XY(int X, int Y);

public interface IUserId
{
    public Guid UserId { get; set; }
}

public interface IUser : IUserId
{
    public string Username { get; set; }
}

public static class UserExtensions
{
    public static UserRecord ToUserRecord(this IUser user) => new(user.UserId, user.Username);
}

public class UserRecord : IUser
{
    public UserRecord(Guid userId, string username)
    {
        UserId = userId;
        Username = username;
    }

    public Guid UserId { get; set; }

    public string Username { get; set; }
}

public record ClientIdentity(string Key, string IpAddress, string Platform);