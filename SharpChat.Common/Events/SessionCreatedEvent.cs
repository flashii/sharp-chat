using SharpChat.Sessions;
using System;
using System.Net;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class SessionCreatedEvent : Event {
        public const string TYPE = @"session:create";

        public DateTimeOffset LastPing { get; }
        public bool IsConnected { get; }
        public IPAddress RemoteAddress { get; }

        public SessionCreatedEvent(ISession session) : base(null, session.User, session, session.Connection) {
            LastPing = session.LastPing;
            IsConnected = session.IsConnected;
            RemoteAddress = session.RemoteAddress;
        }
    }
}
