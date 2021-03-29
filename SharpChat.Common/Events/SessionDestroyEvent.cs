using SharpChat.Sessions;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class SessionDestroyEvent : SessionEvent {
        public const string TYPE = PREFIX + @"destroy";

        public SessionDestroyEvent(ISession session)
            : base(session, true) {}
    }
}
