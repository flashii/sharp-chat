using SharpChat.Sessions;
using System;
using System.Net;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class SessionCreatedEvent : Event {
        public const string TYPE = @"session:create";

        public string ServerId { get; }
        public DateTimeOffset LastPing { get; }
        public bool IsSecure { get; }
        public bool IsConnected { get; }
        public IPAddress RemoteAddress { get; }

        public SessionCreatedEvent(ISession session) : base(session) {
            ServerId = session.ServerId;
            LastPing = session.LastPing;
            IsSecure = session.IsSecure;
            IsConnected = session.IsConnected;
            RemoteAddress = session.RemoteAddress;
        }
    }
}
