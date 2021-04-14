using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class NotOnChannelReply : Reply {
        public const int CODE = 442;

        public override int ReplyCode => CODE;

        private IChannel Channel { get; }

        public NotOnChannelReply(IChannel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        protected override string BuildLine() {
            return $@"{Channel.GetIRCName()} :You're not on that channel";
        }
    }
}
