using SharpChat.Channels;
using SharpChat.Sessions;
using SharpChat.Users;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class ChannelUserJoinEvent : Event {
        public const string TYPE = @"channel:user:join";

        public ChannelUserJoinEvent(IUser user, IChannel channel) : base(user, channel) { }

        public ChannelUserJoinEvent(IChannel channel, ISession session) : base(channel, session) { }
    }
}
