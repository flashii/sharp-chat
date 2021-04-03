using SharpChat.Sessions;
using System;
using System.Net;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class SessionCreatedEvent : SessionEvent {
        public const string TYPE = PREFIX + @"create";

        public DateTimeOffset LastPing { get; }
        public bool IsConnected { get; }
        public IPAddress RemoteAddress { get; }

        public SessionCreatedEvent(ISession session) : base(session, true) {
            LastPing = session.LastPing;
            IsConnected = session.IsConnected;
            RemoteAddress = session.RemoteAddress;
        }
    }
}
