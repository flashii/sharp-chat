using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class NoTopicReply : ServerReply {
        public const int CODE = 331;

        public override int ReplyCode => CODE;

        private IChannel Channel { get; }

        public NoTopicReply(IChannel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        protected override string BuildLine() {
            return $@"{Channel.GetIRCName()} :No topic is set";
        }
    }
}
