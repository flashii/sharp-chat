using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
