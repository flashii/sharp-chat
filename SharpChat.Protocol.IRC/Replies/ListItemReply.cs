using SharpChat.Channels;
using SharpChat.Protocol.IRC.Channels;
using System;
using System.Text;

namespace SharpChat.Protocol.IRC.Replies {
    public class ListItemReply : Reply {
        public const int CODE = 322;

        public override int ReplyCode => CODE;

        private IChannel Channel { get; }
        private int UserCount { get; }

        public ListItemReply(IChannel channel, int userCount) {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));

            // some irc clients don't show channels unless they actually have people in them,
            //  so always show at least 1 user
            UserCount = Math.Max(1, userCount);
        }

        protected override string BuildLine() {
            StringBuilder sb = new();

            sb.Append(Channel.GetIRCName());
            sb.Append(' ');
            sb.Append(UserCount);
            sb.Append(' ');
            sb.Append(':');
            sb.Append(Channel.Topic);

            return sb.ToString();
        }
    }
}
