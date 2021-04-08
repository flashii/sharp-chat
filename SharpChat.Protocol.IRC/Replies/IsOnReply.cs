using System;
using System.Collections.Generic;

namespace SharpChat.Protocol.IRC.Replies {
    public class IsOnReply : Reply {
        public const int CODE = 303;

        public override int ReplyCode => CODE;

        private IEnumerable<string> UserNames { get; }

        public IsOnReply(IEnumerable<string> userNames) {
            UserNames = userNames ?? throw new ArgumentNullException(nameof(userNames));
        }

        protected override string BuildLine() {
            return string.Join(' ', UserNames);
        }
    }
}
