using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class ChannelIsFullReply : ServerReply {
        public const int CODE = 471;

        public override int ReplyCode => CODE;

        private IChannel Channel { get; }

        public ChannelIsFullReply(IChannel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        protected override string BuildLine() {
            return $@"{Channel.GetIRCName()} :Cannot join channel (+l)";
        }
    }
}
