using SharpChat.Channels;
using SharpChat.Sessions;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class ChannelSessionJoinEvent : Event {
        public const string TYPE = @"channel:session:join";

        public ChannelSessionJoinEvent(IChannel channel, ISession session) : base(channel, session) { }
    }
}
