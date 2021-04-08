using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class NoRecipientReply : Reply {
        public const int CODE = 411;

        public override int ReplyCode => CODE;

        private string Command { get; }

        public NoRecipientReply(string command) {
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        protected override string BuildLine() {
            return $@":No recipient given ({Command})";
        }
    }
}
