using System;

namespace SharpChat.Protocol.IRC.Replies {
    public class YouAreBannedReply : Reply {
        public const int CODE = 465;

        public override int ReplyCode => CODE;

        private string Reason { get; }

        public YouAreBannedReply(string reason) {
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        }

        protected override string BuildLine() {
            return $@":{Reason}";
        }
    }
}
