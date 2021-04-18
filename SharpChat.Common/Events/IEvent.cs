using System;

namespace SharpChat.Events {
    public interface IEvent {
        long EventId { get; }
        DateTimeOffset DateTime { get; }
        long UserId { get; }
        string ChannelId { get; }
        string SessionId { get; }
        string ConnectionId { get; }
    }
}
