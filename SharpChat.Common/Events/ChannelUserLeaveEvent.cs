using SharpChat.Channels;
using SharpChat.Users;
using System;

namespace SharpChat.Events {
    [Event(TYPE)]
    public class ChannelUserLeaveEvent : Event {
        public const string TYPE = @"channel:user:leave";

        public UserDisconnectReason Reason { get; }

        public ChannelUserLeaveEvent(IUser user, IChannel channel, UserDisconnectReason reason)
            : base(user, channel) {
            Reason = reason;
        }
    }
}
