using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class NoSuchNickReply : ServerReply {
        public const int CODE = 401;

        public override int ReplyCode => CODE;

        private string UserName { get; }

        public NoSuchNickReply(string userName) {
            UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        }

        protected override string BuildLine() {
            return $@"{UserName} :User not found.";
        }
    }
}
