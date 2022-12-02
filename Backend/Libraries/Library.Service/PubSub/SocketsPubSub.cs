
using Library.Core;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;

namespace Library.Service.PubSub;

public interface ISendMessageRq
{
    string Method { get; }
    Guid? UserId { get; }
    object Data { get; }
}

public class SendMessageRq : ISendMessageRq
{
    public string Method { get; init; }
    public Guid? UserId { get; init; }
    public UserLevel? UserLevel { get; init; }
    public object Data { get; init; }

    public SendMessageRq() { }

    public SendMessageRq(string method, Guid? userId, UserLevel? level, object data)
    {
        Method = method;
        UserId = userId;
        Data = data;
        UserLevel = level;
    }

    public SendMessageRq(string method, Guid userId, object data) : this(method, userId, null, data) { }

    public SendMessageRq(string method, UserLevel level, object data) : this(method, null, level, data) { }

    public SendMessageRq(string method, object data) : this(method, null, null, data) { }

    public SendMessageRq(string method, Guid userId) : this(method, userId, null, null) { }

    public SendMessageRq(string method) : this(method, null, null, null) { }
}

public interface ISocketsPubSub
{
    Task Send(SendMessageRq payload);
}

public class SocketsPubSub : BasePubSub, ISocketsPubSub
{
    public SocketsPubSub(IBus publishEndpoint, IServiceScopeFactory scopeFactory) : base(publishEndpoint, scopeFactory)
    {
    }

    public async Task Send(SendMessageRq payload) => await Publish(payload);
}