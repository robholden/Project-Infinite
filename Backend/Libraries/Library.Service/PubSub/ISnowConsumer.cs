using Library.Core;

using MassTransit;

namespace Library.Service.PubSub;

public interface ISnowConsumer : IConsumer
{
}

public class SnowConsumerResponse
{
    public ErrorCodeDto Error { get; set; }

    public static SnowConsumerResponse Ok() => new();

    public static SnowConsumerResponse Throw(Exception exception)
    {
        if (exception is not SiteException siteExc)
        {
            siteExc = new SiteException(exception.Message);
        }

        return new() { Error = siteExc.ToDto() };
    }
}

public class SnowConsumerResponse<T> : SnowConsumerResponse
{
    public T Result { get; set; }

    public static SnowConsumerResponse<T> FromResult(T result) => new() { Result = result };
}