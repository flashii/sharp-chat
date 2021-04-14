using SharpChat.Protocol;
using SharpChat.Sessions;
using System;
using System.Net;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class SessionResumeEvent : Event {
        public const string TYPE = @"session:resume";

        public string ServerId { get; }
        public IPAddress RemoteAddress { get; }

        public bool HasConnection
            => Connection != null;

        public SessionResumeEvent(ISession session, string serverId, IPAddress remoteAddress)
            : base(null, session.User, session, session.Connection) {
            ServerId = serverId ?? throw new ArgumentNullException(nameof(serverId));
            RemoteAddress = remoteAddress ?? throw new ArgumentNullException(nameof(remoteAddress));
        }

        public SessionResumeEvent(ISession session, IConnection connection, string serverId)
            : this(session, serverId, connection.RemoteAddress) { }
    }
}
