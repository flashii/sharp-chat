using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class EndOfNamesReply : ServerReply {
        public const int CODE = 366;

        public override int ReplyCode => CODE;

        private IChannel Channel { get; }

        public EndOfNamesReply(IChannel channel) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        protected override string BuildLine() {
            return $@"{Channel.GetIRCName()} :End of NAMES list";
        }
    }
}
