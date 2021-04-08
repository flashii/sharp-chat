using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using SharpChat.Protocol.IRC.Users;
using SharpChat.Users;
using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class UserOnChannelReply : Reply {
        public const int CODE = 443;

        public override int ReplyCode => CODE;

        private IUser User { get; }
        private IChannel Channel { get; }

        public UserOnChannelReply(IUser user, IChannel channel) {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        protected override string BuildLine() {
            return $@"{User.GetIRCName()} {Channel.GetIRCName()} :is already on channel";
        }
    }
}
