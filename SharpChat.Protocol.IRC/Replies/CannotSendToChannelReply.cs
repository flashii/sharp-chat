using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class CannotSendToChannelReply : Reply {
        public const int CODE = 404;

        public override int ReplyCode => CODE;

        private IChannel Channel { get; }

        public CannotSendToChannelReply(IChannel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        protected override string BuildLine() {
            return $@"{Channel.GetIRCName()} :Cannot send to channel";
        }
    }
}
