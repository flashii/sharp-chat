using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class BadChannelKeyReply : ServerReply {
        public const int CODE = 475;

        public override int ReplyCode => CODE;

        private IChannel Channel { get; }

        public BadChannelKeyReply(IChannel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        protected override string BuildLine() {
            return $@"{Channel.GetIRCName()} :Cannot join channel (+k)";
        }
    }
}
