using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class MotdReply : Reply {
        public const int CODE = 372;

        public override int ReplyCode => CODE;

        private string Line { get; }

        public MotdReply(string line) {
            Line = line ?? throw new ArgumentNullException(nameof(line));
        }

        protected override string BuildLine() {
            return Line;
        }
    }
}
