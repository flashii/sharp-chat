using SharpChat.Channels;
using SharpChat.Protocol;
using SharpChat.Sessions;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    public interface IEvent {
        long EventId { get; }
        DateTimeOffset DateTime { get; }
        IUser User { get; }
        IChannel Channel { get; }
        ISession Session { get; }
        IConnection Connection { get; }
    }
}
