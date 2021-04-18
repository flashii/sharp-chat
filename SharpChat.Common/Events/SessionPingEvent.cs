using SharpChat.Sessions;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class SessionPingEvent : Event {
        public const string TYPE = @"session:ping";

        public SessionPingEvent(ISession session)
            : base(session) { }
    }
}
