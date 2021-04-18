using SharpChat.Channels;
using SharpChat.Protocol;
using SharpChat.Sessions;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public abstract class Event : IEvent {
        public long EventId { get; }
        public DateTimeOffset DateTime { get; }
        public long UserId { get; }
        public string ChannelName { get; }
        public string SessionId { get; }
        public string ConnectionId { get; }

        public Event(IChannel channel, IUser user, ISession session = null, IConnection conn = null, DateTimeOffset? dateTime = null) {
            EventId = SharpId.Next();
            DateTime = dateTime ?? DateTimeOffset.Now;
            UserId = user?.UserId ?? -1;
            ChannelName = channel?.Name ?? string.Empty;
            SessionId = session?.SessionId ?? string.Empty;
            ConnectionId = conn?.ConnectionId ?? string.Empty;
        }

        public override string ToString() {
            return $@"[{EventId}:{GetType().Name}] U:{UserId} Ch:{ChannelName} S:{SessionId} Co:{ConnectionId}";
        }
    }
}
