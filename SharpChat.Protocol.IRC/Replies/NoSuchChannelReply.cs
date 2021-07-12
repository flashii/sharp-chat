using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class NoSuchChannelReply : Reply {
        public const int CODE = 403;

        public override int ReplyCode => CODE;

        private string ChannelName { get; }

        public NoSuchChannelReply(string channelName) {
            ChannelName = channelName ?? throw new ArgumentNullException(nameof(channelName));
        }

        protected override string BuildLine() {
            return $@"{ChannelName} :Channel not found.";
        }
    }
}
