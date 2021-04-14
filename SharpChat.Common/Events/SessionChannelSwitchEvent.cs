using SharpChat.Channels;
using SharpChat.Sessions;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class SessionChannelSwitchEvent : Event {
        public const string TYPE = @"session:channel:switch";

        public SessionChannelSwitchEvent(ISession session, IChannel channel)
            : base(channel, session.User, session, session.Connection) { }
    }
}
