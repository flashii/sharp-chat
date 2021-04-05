using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class BadChannelMaskReply : ServerReply {
        public const int CODE = 476;

        public override int ReplyCode => CODE;

        private string ChannelName { get; }

        public BadChannelMaskReply(string channelName) {
            ChannelName = channelName ?? throw new ArgumentNullException(nameof(channelName));
        }

        protected override string BuildLine() {
            return $@"{ChannelName} :Bad Channel Mask";
        }
    }
}
