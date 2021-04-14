using SharpChat.Sessions;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class SessionSuspendEvent : Event {
        public const string TYPE = @"session:suspend";

        public SessionSuspendEvent(ISession session)
            : base(null, session.User, session, session.Connection) { }
    }
}
