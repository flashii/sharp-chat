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
        public string ChannelId { get; }
        public string SessionId { get; }
        public string ConnectionId { get; }

        public Event(long eventId, DateTimeOffset dateTime, long userId, string channelId, string sessionId, string connectionId) {
            EventId = eventId;
            DateTime = dateTime;
            UserId = userId;
            ChannelId = channelId ?? string.Empty;
            SessionId = sessionId ?? string.Empty;
            ConnectionId = connectionId ?? string.Empty;
        }

        public Event(DateTimeOffset dateTime, long userId, string channelId, string sessionId, string connectionId)
            : this(SharpId.Next(), dateTime, userId, channelId, sessionId, connectionId) { }

        public Event(long userId, string channelId, string sessionId, string connectionId)
            : this(DateTimeOffset.Now, userId, channelId, sessionId, connectionId) { }

        public Event(string channelName, string sessionId, string connectionId)
            : this(-1L, channelName, sessionId, connectionId) { }

        public Event(IUser user, IChannel channel, ISession session, IConnection connection)
            : this(user?.UserId ?? -1L, channel?.ChannelId, session?.SessionId, connection?.ConnectionId) { }

        public Event(IUser user, ISession session, IConnection connection)
            : this(user, null, session, connection) { }

        public Event(IUser user, IChannel channel, ISession session)
            : this(user, channel, session, session?.Connection) { }

        public Event(IUser user, IChannel channel)
            : this(user, channel, null, null) { }

        public Event(long userId, IChannel channel)
            : this(userId, channel.ChannelId, null, null) { }

        public Event(IChannel channel, ISession session)
            : this(session?.User, channel, session, session?.Connection) { }

        public Event(ISession session, IConnection connection)
            : this(session?.User, null, session, connection) { }

        public Event(IUser user)
            : this(user, null, null, null) { }

        public Event(long userId)
            : this(userId, null, null, null) { }

        public Event(IChannel channel)
            : this(null, channel, null, null) { }

        public Event(ISession session)
            : this(session?.User, null, session, session?.Connection) { }

        public Event(IConnection connection)
            : this(connection?.Session?.User, null, connection?.Session, connection) { }

        public Event()
            : this(-1L, null, null, null) { }

        public override string ToString()
            => $@"[{EventId}:{GetType().Name}] U:{UserId} Ch:{ChannelId} S:{SessionId} Co:{ConnectionId}";
    }
}
