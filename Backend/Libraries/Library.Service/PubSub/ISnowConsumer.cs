using Library.Core;

using MassTransit;

namespace Library.Service.PubSub;

public interface ISnowConsumer : IConsumer
{
}


public class SnowConsumerResponse
{
    public SnowConsumerResponse(Exception exception = null)
    {
        if (exception == null) return;

        if (exception is SiteException siteExc) Error = siteExc?.ToDto();
        else Error = new SiteException(exception.Message).ToDto();
    }

    public ErrorCodeDto Error { get; set; }

    public static SnowConsumerResponse Ok() => new();

    public static SnowConsumerResponse Bad(SiteException exception) => new(exception);
}

public class SnowConsumerResponse<T> : SnowConsumerResponse
{
    public T Result { get; set; }

    public SnowConsumerResponse(T result, SiteException exception = null) : base(exception)
    {
        Result = result;
    }

    public static SnowConsumerResponse<T> FromResult(T result) => new(result);
}