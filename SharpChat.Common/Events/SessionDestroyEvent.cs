using SharpChat.Sessions;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class SessionDestroyEvent : Event {
        public const string TYPE = @"session:destroy";

        public SessionDestroyEvent(ISession session)
            : base(null, session.User, session, session.Connection) {}
    }
}
