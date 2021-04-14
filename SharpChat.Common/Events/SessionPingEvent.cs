using SharpChat.Sessions;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class SessionPingEvent : Event {
        public const string TYPE = @"session:ping";

        public SessionPingEvent(ISession session)
            : base(null, session.User, session, session.Connection) { }
    }
}
