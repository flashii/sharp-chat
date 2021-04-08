using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class TopicReply : Reply {
        public const int CODE = 332;

        public override int ReplyCode => CODE;

        private IChannel Channel { get; }

        public TopicReply(IChannel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        protected override string BuildLine() {
            return $@"{Channel.GetIRCName()} :{Channel.Topic}";
        }
    }
}
