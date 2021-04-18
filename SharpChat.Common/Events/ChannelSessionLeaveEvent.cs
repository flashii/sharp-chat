using SharpChat.Channels;
using SharpChat.Sessions;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class ChannelSessionLeaveEvent : Event {
        public const string TYPE = @"channel:session:leave";

        public ChannelSessionLeaveEvent(IChannel channel, ISession session) : base(channel, session) { }
    }
}
