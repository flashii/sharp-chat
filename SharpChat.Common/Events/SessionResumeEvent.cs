using SharpChat.Protocol;
using SharpChat.Sessions;
using System;
using System.Net;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class SessionResumeEvent : SessionEvent {
        public const string TYPE = PREFIX + @"resume";

        public IConnection Connection { get; } // should this be carried by an event?
        public string ServerId { get; }
        public IPAddress RemoteAddress { get; }

        public bool HasConnection
            => Connection != null;

        public SessionResumeEvent(ISession session, string serverId, IPAddress remoteAddress)
            : base(session, false, null) {
            ServerId = serverId ?? throw new ArgumentNullException(nameof(serverId));
            RemoteAddress = remoteAddress ?? throw new ArgumentNullException(nameof(remoteAddress));
        }

        public SessionResumeEvent(ISession session, IConnection connection, string serverId)
            : this(session, serverId, connection.RemoteAddress) {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }
    }
}
