using SharpChat.Channels;
using SharpChat.Users;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class ChannelDeleteEvent : Event {
        public const string TYPE = @"channel:delete";

        public ChannelDeleteEvent(IChannel channel) : base(channel) { }

        public ChannelDeleteEvent(IUser user, IChannel channel) : base(user, channel) { }
    }
}
