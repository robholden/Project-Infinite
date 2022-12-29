using Library.Core;

using MassTransit;

namespace Library.Service.PubSub;

public interface IRabbitConsumer : IConsumer
{
}

public class RabbitConsumerResponse
{
    public ErrorCodeDto Error { get; set; }

    public static RabbitConsumerResponse Ok() => new();

    public static RabbitConsumerResponse Throw(Exception exception)
    {
        if (exception is not SiteException siteExc)
        {
            siteExc = new SiteException(exception.Message);
        }

        return new() { Error = siteExc.ToDto() };
    }
}

public class RabbitConsumerResponse<T> : RabbitConsumerResponse
{
    public T Result { get; set; }

    public static RabbitConsumerResponse<T> FromResult(T result) => new() { Result = result };
}