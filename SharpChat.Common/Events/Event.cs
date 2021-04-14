using SharpChat.Channels;
using SharpChat.Protocol;
using SharpChat.Sessions;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public abstract class Event : IEvent {
        public long EventId { get; }
        public DateTimeOffset DateTime { get; }
        public IUser User { get; }
        public IChannel Channel { get; }
        public ISession Session { get; }
        public IConnection Connection { get; }

        public Event(IChannel channel, IUser user, ISession session = null, IConnection conn = null, DateTimeOffset? dateTime = null) {
            EventId = SharpId.Next();
            DateTime = dateTime ?? DateTimeOffset.Now;
            User = user;
            Channel = channel;
            Session = session;
            Connection = conn;
        }

        public override string ToString() {
            return $@"[{EventId}] {GetType().Name} {User} {Channel} {Session} {Connection}";
        }
    }
}
